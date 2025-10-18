/**
 * TraceViewer - Handles Server-Sent Events (SSE) streaming of trace data
 *
 * Connects to backend SSE endpoint and forwards trace updates to Blazor component.
 */

class TraceViewer {
    constructor() {
        this.eventSource = null;
        this.sessionId = null;
        this.dotNetHelper = null;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 1000; // Start with 1 second
    }

    /**
     * Start streaming traces for a session
     * @param {string} sessionId - Session identifier
     * @param {string} apiBaseUrl - Base URL of Python API
     * @param {any} dotNetHelper - Blazor DotNetObjectReference for callbacks
     */
    start(sessionId, apiBaseUrl, dotNetHelper) {
        console.log(`[TraceViewer] Starting for session: ${sessionId}`);

        this.sessionId = sessionId;
        this.dotNetHelper = dotNetHelper;

        // Close existing connection if any
        this.stop();

        // Construct SSE endpoint URL
        const url = `${apiBaseUrl}/api/session/${sessionId}/traces/stream`;
        console.log(`[TraceViewer] Connecting to: ${url}`);

        try {
            this.eventSource = new EventSource(url);

            this.eventSource.onopen = () => {
                console.log(`[TraceViewer] Connected successfully`);
                this.reconnectAttempts = 0; // Reset on successful connection
                this.reconnectDelay = 1000;
            };

            this.eventSource.onmessage = (event) => {
                try {
                    const traceData = JSON.parse(event.data);
                    console.log(`[TraceViewer] Received trace:`, traceData);

                    // Forward to Blazor component
                    if (this.dotNetHelper) {
                        this.dotNetHelper.invokeMethodAsync('OnTraceUpdate', traceData);
                    }
                } catch (error) {
                    console.error(`[TraceViewer] Failed to parse trace data:`, error);
                }
            };

            this.eventSource.onerror = (error) => {
                console.error(`[TraceViewer] Connection error:`, error);

                // Close the connection
                this.eventSource.close();

                // Attempt reconnection with exponential backoff
                if (this.reconnectAttempts < this.maxReconnectAttempts) {
                    this.reconnectAttempts++;
                    const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts - 1);

                    console.log(`[TraceViewer] Reconnecting in ${delay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`);

                    setTimeout(() => {
                        if (this.sessionId && this.dotNetHelper) {
                            this.start(this.sessionId, apiBaseUrl, this.dotNetHelper);
                        }
                    }, delay);
                } else {
                    console.error(`[TraceViewer] Max reconnect attempts reached`);

                    // Notify Blazor of connection failure
                    if (this.dotNetHelper) {
                        this.dotNetHelper.invokeMethodAsync('OnConnectionError');
                    }
                }
            };

        } catch (error) {
            console.error(`[TraceViewer] Failed to start:`, error);

            // Notify Blazor of connection failure
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnConnectionError');
            }
        }
    }

    /**
     * Stop streaming and close connection
     */
    stop() {
        if (this.eventSource) {
            console.log(`[TraceViewer] Stopping connection`);
            this.eventSource.close();
            this.eventSource = null;
        }

        this.sessionId = null;
        this.dotNetHelper = null;
        this.reconnectAttempts = 0;
    }

    /**
     * Check if currently connected
     * @returns {boolean} True if connected
     */
    isConnected() {
        return this.eventSource !== null && this.eventSource.readyState === EventSource.OPEN;
    }
}

// Global instance
window.traceViewer = new TraceViewer();

// Export for use in Blazor
window.startTraceViewer = (sessionId, apiBaseUrl, dotNetHelper) => {
    window.traceViewer.start(sessionId, apiBaseUrl, dotNetHelper);
};

window.stopTraceViewer = () => {
    window.traceViewer.stop();
};

window.isTraceViewerConnected = () => {
    return window.traceViewer.isConnected();
};
