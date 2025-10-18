/**
 * TailorBlend SSE Client - JavaScript EventSource Implementation
 *
 * Handles Server-Sent Events (SSE) streaming from Python FastAPI backend.
 * Provides clean integration with Blazor via DotNetObjectReference callbacks.
 *
 * Features:
 * - Native browser EventSource API (automatic reconnection)
 * - Proper error handling and categorization
 * - Memory leak prevention (cleanup on close)
 * - Comprehensive logging for debugging
 */

window.sseClient = {
    /**
     * Active EventSource connections tracked by request ID
     * @type {Map<string, {eventSource: EventSource, timeout: number}>}
     */
    activeConnections: new Map(),

    /**
     * Start streaming chat response via EventSource
     *
     * @param {string} url - SSE endpoint URL (e.g., "/api/chat/stream?message=...")
     * @param {object} dotNetRef - DotNetObjectReference for callbacks to C#
     * @param {string} requestId - Unique identifier for this streaming request
     * @returns {Promise<void>}
     */
    async connect(url, dotNetRef, requestId) {
        console.group(`[SSE] Starting stream ${requestId}`);
        console.log(`URL: ${url}`);
        console.log(`Time: ${new Date().toISOString()}`);
        console.groupEnd();

        // Close any existing connection with this requestId
        if (this.activeConnections.has(requestId)) {
            console.warn(`[SSE] Request ${requestId} already active, closing previous connection`);
            this.disconnect(requestId);
        }

        try {
            // Create EventSource connection
            const eventSource = new EventSource(url);

            // Set 5-minute timeout for the entire stream
            const timeoutId = setTimeout(() => {
                console.error(`[SSE] Stream ${requestId} timed out after 5 minutes`);
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnStreamError', 'Stream timed out after 5 minutes')
                        .catch(err => console.error(`[SSE] Error calling OnStreamError:`, err));
                }
                this.disconnect(requestId);
            }, 5 * 60 * 1000); // 5 minutes

            this.activeConnections.set(requestId, { eventSource, timeoutId });

            // Connection opened successfully
            eventSource.onopen = () => {
                console.log(`[SSE] Connection opened for request ${requestId}`);

                // Notify C# that connection is established
                if (dotNetRef) {
                    try {
                        dotNetRef.invokeMethodAsync('OnConnected');
                    } catch (err) {
                        console.error(`[SSE] Error calling OnConnected:`, err);
                    }
                }
            };

            // Token received from server
            eventSource.onmessage = (event) => {
                const data = event.data;

                // Check for completion signal
                if (data === '[DONE]') {
                    console.log(`[SSE] Stream ${requestId} completed (received [DONE] signal)`);

                    // Notify C# of stream completion
                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnStreamComplete')
                            .then(() => {
                                // Close connection after successful completion
                                this.disconnect(requestId);
                            })
                            .catch(err => {
                                console.error(`[SSE] Error calling OnStreamComplete:`, err);
                                this.disconnect(requestId);
                            });
                    } else {
                        // DotNetRef already disposed, just close connection
                        this.disconnect(requestId);
                    }

                    return;
                }

                // Parse JSON token (backend sends JSON-encoded strings)
                let token;
                try {
                    token = JSON.parse(data);
                } catch (parseError) {
                    // If not JSON, use raw data
                    console.warn(`[SSE] Non-JSON data received, using raw:`, data);
                    token = data;
                }

                // Send token to C#
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnTokenReceived', token)
                        .catch(err => {
                            console.error(`[SSE] Error calling OnTokenReceived:`, err);
                        });
                }
            };

            // Error handling
            eventSource.onerror = (error) => {
                console.error(`[SSE] Error for request ${requestId}:`, error);

                // Check connection state
                if (eventSource.readyState === EventSource.CONNECTING) {
                    // EventSource is attempting to reconnect (browser handles this automatically)
                    console.log(`[SSE] EventSource attempting to reconnect...`);

                } else if (eventSource.readyState === EventSource.CLOSED) {
                    // Connection permanently closed
                    console.error(`[SSE] Connection ${requestId} closed permanently`);

                    // Notify C# of error
                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnStreamError', 'Connection closed unexpectedly')
                            .catch(err => {
                                console.error(`[SSE] Error calling OnStreamError:`, err);
                            })
                            .finally(() => {
                                // Clean up connection
                                this.disconnect(requestId);
                            });
                    } else {
                        // DotNetRef already disposed, just close connection
                        this.disconnect(requestId);
                    }
                }
                // EventSource.OPEN state should not trigger onerror
            };

        } catch (err) {
            console.error(`[SSE] Failed to create EventSource for ${requestId}:`, err);

            // Notify C# of initialization error
            if (dotNetRef) {
                try {
                    await dotNetRef.invokeMethodAsync('OnStreamError', `Failed to initialize: ${err.message}`);
                } catch (callbackErr) {
                    console.error(`[SSE] Error calling OnStreamError:`, callbackErr);
                }
            }

            // Clean up
            this.disconnect(requestId);
        }
    },

    /**
     * Disconnect and cleanup a specific EventSource connection
     *
     * @param {string} requestId - Request ID to disconnect
     */
    disconnect(requestId) {
        const connection = this.activeConnections.get(requestId);

        if (!connection) {
            console.warn(`[SSE] No active connection found for request ${requestId}`);
            return;
        }

        console.log(`[SSE] Closing connection for request ${requestId}`);

        const { eventSource, timeoutId } = connection;

        // Clear the timeout
        if (timeoutId) {
            clearTimeout(timeoutId);
        }

        // Remove event listeners to prevent memory leaks
        eventSource.onopen = null;
        eventSource.onmessage = null;
        eventSource.onerror = null;

        // Close the connection
        eventSource.close();

        // Remove from tracking
        this.activeConnections.delete(requestId);

        console.log(`[SSE] Connection ${requestId} closed and cleaned up`);
    },

    /**
     * Disconnect all active EventSource connections
     * Used during component disposal or emergency cleanup
     */
    disconnectAll() {
        console.log(`[SSE] Closing all active connections (${this.activeConnections.size} active)`);

        // Close each connection
        for (const [requestId, connection] of this.activeConnections.entries()) {
            console.log(`[SSE] Closing connection: ${requestId}`);

            const { eventSource, timeoutId } = connection;

            // Clear timeout
            if (timeoutId) {
                clearTimeout(timeoutId);
            }

            // Remove event listeners
            eventSource.onopen = null;
            eventSource.onmessage = null;
            eventSource.onerror = null;

            // Close connection
            eventSource.close();
        }

        // Clear tracking map
        this.activeConnections.clear();

        console.log(`[SSE] All connections closed`);
    },

    /**
     * Check if EventSource is supported by this browser
     * @returns {boolean}
     */
    isSupported() {
        return typeof EventSource !== 'undefined';
    },

    /**
     * Get count of active connections (for debugging)
     * @returns {number}
     */
    getActiveConnectionCount() {
        return this.activeConnections.size;
    }
};

// Log successful initialization
console.log('[SSE] sse-client.js loaded successfully');
console.log(`[SSE] EventSource supported: ${window.sseClient.isSupported()}`);
