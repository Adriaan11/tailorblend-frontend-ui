/**
 * TailorBlend Mobile Utilities
 *
 * Mobile-first enhancements for optimal UX on phones and tablets
 */

(function () {
    'use strict';

    // ============================================================================
    // 1. VIRTUAL KEYBOARD DETECTION & HANDLING
    // ============================================================================

    /**
     * Detects when the virtual keyboard appears/disappears on mobile
     * Adjusts UI accordingly to prevent input fields from being hidden
     */
    const KeyboardManager = {
        isKeyboardVisible: false,
        originalViewportHeight: window.visualViewport ? window.visualViewport.height : window.innerHeight,
        bottomNav: null,
        initialized: false,

        init: function() {
            // Guard against duplicate initialization
            if (this.initialized) {
                console.log('[KeyboardManager] Already initialized, skipping...');
                return;
            }

            this.bottomNav = document.querySelector('.tb-bottom-nav');

            if (window.visualViewport) {
                // Modern API (iOS Safari 13+, Chrome 61+)
                window.visualViewport.addEventListener('resize', () => this.handleViewportResize());
                window.visualViewport.addEventListener('scroll', () => this.handleViewportScroll());
            } else {
                // Fallback for older browsers
                window.addEventListener('resize', () => this.handleWindowResize());
            }

            // Listen for focus on input fields
            document.addEventListener('focusin', (e) => this.handleInputFocus(e));
            document.addEventListener('focusout', (e) => this.handleInputBlur(e));

            this.initialized = true;
        },

        handleViewportResize: function() {
            const viewport = window.visualViewport;
            const currentHeight = viewport.height;
            const heightDiff = this.originalViewportHeight - currentHeight;

            // Keyboard is visible if viewport shrinks by more than 150px
            const keyboardVisible = heightDiff > 150;

            if (keyboardVisible !== this.isKeyboardVisible) {
                this.isKeyboardVisible = keyboardVisible;
                this.adjustUI(keyboardVisible);
            }
        },

        handleViewportScroll: function() {
            if (this.isKeyboardVisible) {
                // Ensure input remains visible when viewport scrolls
                const activeElement = document.activeElement;
                if (activeElement && (activeElement.tagName === 'INPUT' || activeElement.tagName === 'TEXTAREA')) {
                    setTimeout(() => {
                        activeElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }, 100);
                }
            }
        },

        handleWindowResize: function() {
            // Fallback for browsers without visualViewport API
            const currentHeight = window.innerHeight;
            const heightDiff = this.originalViewportHeight - currentHeight;
            const keyboardVisible = heightDiff > 150;

            if (keyboardVisible !== this.isKeyboardVisible) {
                this.isKeyboardVisible = keyboardVisible;
                this.adjustUI(keyboardVisible);
            }
        },

        handleInputFocus: function(e) {
            const target = e.target;
            if (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA') {
                // Add focused class for styling
                target.classList.add('tb-input-focused');

                // Scroll into view after a short delay (wait for keyboard animation)
                setTimeout(() => {
                    if (document.activeElement === target) {
                        target.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }
                }, 300);
            }
        },

        handleInputBlur: function(e) {
            const target = e.target;
            if (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA') {
                target.classList.remove('tb-input-focused');
            }
        },

        adjustUI: function(keyboardVisible) {
            document.body.classList.toggle('tb-keyboard-visible', keyboardVisible);

            if (keyboardVisible) {
                // Hide bottom nav when keyboard appears (more screen space for content)
                if (this.bottomNav) {
                    this.bottomNav.style.transform = 'translateY(100%)';
                    this.bottomNav.style.transition = 'transform 0.3s ease';
                }

                // Adjust chat input container
                const chatInputContainer = document.querySelector('.tb-chat-input-container');
                if (chatInputContainer) {
                    chatInputContainer.style.paddingBottom = '8px';
                }
            } else {
                // Show bottom nav when keyboard disappears
                if (this.bottomNav) {
                    this.bottomNav.style.transform = 'translateY(0)';
                }

                // Reset chat input padding
                const chatInputContainer = document.querySelector('.tb-chat-input-container');
                if (chatInputContainer) {
                    chatInputContainer.style.paddingBottom = 'calc(var(--tb-space-12) + var(--safe-area-inset-bottom))';
                }
            }

            // Dispatch custom event for Blazor components to listen to
            window.dispatchEvent(new CustomEvent('keyboardVisibilityChanged', {
                detail: { visible: keyboardVisible }
            }));
        }
    };

    // ============================================================================
    // 2. AUTO-RESIZE TEXTAREA
    // ============================================================================

    /**
     * Automatically resizes textarea as user types
     * Prevents awkward scrolling within small textarea
     */
    window.autoResizeTextarea = function(element) {
        if (!element) return;

        element.style.height = 'auto';
        const newHeight = Math.min(element.scrollHeight, 120); // Max 120px
        element.style.height = newHeight + 'px';

        // Scroll to keep textarea visible if it's growing
        setTimeout(() => {
            element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }, 50);
    };

    /**
     * Initialize auto-resize for all textareas with .tb-chat-input class
     * Idempotent - checks for existing listeners before adding
     */
    function initAutoResizeTextareas() {
        const textareas = document.querySelectorAll('.tb-chat-input');
        textareas.forEach(textarea => {
            // Check if already initialized
            if (!textarea.dataset.autoResizeInitialized) {
                textarea.addEventListener('input', function() {
                    window.autoResizeTextarea(this);
                });
                textarea.dataset.autoResizeInitialized = 'true';
            }
        });
    }

    // ============================================================================
    // 3. HAPTIC FEEDBACK
    // ============================================================================

    /**
     * Provides haptic feedback on touch interactions
     * Uses Vibration API for tactile response
     */
    window.vibrate = function(pattern) {
        if ('vibrate' in navigator) {
            navigator.vibrate(pattern || 10); // Default: 10ms light tap
        }
    };

    /**
     * Add haptic feedback to all buttons and interactive elements
     */
    function initHapticFeedback() {
        // Light tap on all buttons
        document.addEventListener('click', function(e) {
            const button = e.target.closest('button, .tb-btn, .tb-icon-btn, .tb-quick-action, .tb-bottom-nav__item');
            if (button && !button.disabled) {
                window.vibrate(10); // Light tap
            }
        }, { passive: true });

        // Slightly stronger feedback for important actions
        document.addEventListener('click', function(e) {
            const importantButton = e.target.closest('.tb-chat-send-btn, .tb-btn--primary');
            if (importantButton && !importantButton.disabled) {
                window.vibrate([10, 20, 10]); // Double tap pattern
            }
        }, { passive: true });
    }

    // ============================================================================
    // 4. PULL-TO-REFRESH
    // ============================================================================

    /**
     * Native pull-to-refresh gesture for chat messages
     * Reloads conversation history
     */
    const PullToRefresh = {
        startY: 0,
        currentY: 0,
        isDragging: false,
        threshold: 80,
        container: null,
        refreshIndicator: null,
        initialized: false,

        init: function() {
            // Guard against duplicate initialization
            if (this.initialized) {
                console.log('[PullToRefresh] Already initialized, skipping...');
                return;
            }

            this.container = document.querySelector('.tb-chat-messages');
            if (!this.container) return;

            // Create refresh indicator
            this.createRefreshIndicator();

            // Touch events
            this.container.addEventListener('touchstart', (e) => this.handleTouchStart(e), { passive: true });
            this.container.addEventListener('touchmove', (e) => this.handleTouchMove(e), { passive: false });
            this.container.addEventListener('touchend', (e) => this.handleTouchEnd(e), { passive: true });

            this.initialized = true;
        },

        createRefreshIndicator: function() {
            this.refreshIndicator = document.createElement('div');
            this.refreshIndicator.className = 'tb-pull-refresh-indicator';
            this.refreshIndicator.innerHTML = `
                <div class="tb-spinner" style="width: 24px; height: 24px; border-width: 2px;"></div>
            `;
            this.refreshIndicator.style.cssText = `
                position: absolute;
                top: -60px;
                left: 50%;
                transform: translateX(-50%);
                opacity: 0;
                transition: all 0.3s ease;
            `;

            if (this.container) {
                this.container.style.position = 'relative';
                this.container.insertBefore(this.refreshIndicator, this.container.firstChild);
            }
        },

        handleTouchStart: function(e) {
            // Only trigger if scrolled to top
            if (this.container.scrollTop === 0) {
                this.startY = e.touches[0].pageY;
                this.isDragging = true;
            }
        },

        handleTouchMove: function(e) {
            if (!this.isDragging) return;

            this.currentY = e.touches[0].pageY;
            const diff = this.currentY - this.startY;

            // Only allow pull down (positive diff) when at top
            if (diff > 0 && this.container.scrollTop === 0) {
                e.preventDefault(); // Prevent default scroll

                const pullDistance = Math.min(diff, this.threshold);
                const opacity = Math.min(pullDistance / this.threshold, 1);

                // Update indicator
                this.refreshIndicator.style.transform = `translateX(-50%) translateY(${pullDistance}px)`;
                this.refreshIndicator.style.opacity = opacity;
            }
        },

        handleTouchEnd: function(e) {
            if (!this.isDragging) return;

            const diff = this.currentY - this.startY;

            if (diff > this.threshold) {
                // Trigger refresh
                this.triggerRefresh();
            } else {
                // Reset indicator
                this.resetIndicator();
            }

            this.isDragging = false;
            this.startY = 0;
            this.currentY = 0;
        },

        triggerRefresh: function() {
            // Show loading state
            this.refreshIndicator.style.top = '16px';
            this.refreshIndicator.style.opacity = '1';

            // Dispatch event for Blazor to handle
            window.dispatchEvent(new CustomEvent('pullToRefresh'));

            // Reset after 1 second
            setTimeout(() => {
                this.resetIndicator();
            }, 1000);
        },

        resetIndicator: function() {
            this.refreshIndicator.style.transform = 'translateX(-50%) translateY(0)';
            this.refreshIndicator.style.top = '-60px';
            this.refreshIndicator.style.opacity = '0';
        }
    };

    // ============================================================================
    // 5. CAMERA ACCESS FOR FILE UPLOAD
    // ============================================================================

    /**
     * Enhanced file input to support camera on mobile
     * Allows direct photo/video capture
     */
    window.enableCameraCapture = function() {
        const fileInput = document.querySelector('input[type="file"][data-tb-file-input="true"]');
        if (!fileInput) return;

        // Add capture attribute for mobile camera access
        fileInput.setAttribute('capture', 'environment'); // Use rear camera by default
        fileInput.setAttribute('accept', 'image/*,video/*,.pdf,.txt,.csv,.xlsx,.docx');
    };

    /**
     * Show action sheet for file upload options (Camera, Gallery, Files)
     */
    window.showFileUploadOptions = function() {
        // This would ideally show a native action sheet
        // For now, we'll just trigger the file input with camera support
        window.enableCameraCapture();
        window.triggerFileInput();
    };

    // ============================================================================
    // 6. TOUCH GESTURE ENHANCEMENTS
    // ============================================================================

    /**
     * Long-press detection for context menus on messages
     */
    const LongPressDetector = {
        pressTimer: null,
        threshold: 500, // 500ms for long press
        initialized: false,

        init: function() {
            // Guard against duplicate initialization
            if (this.initialized) {
                console.log('[LongPressDetector] Already initialized, skipping...');
                return;
            }

            document.addEventListener('touchstart', (e) => this.handleTouchStart(e), { passive: true });
            document.addEventListener('touchend', (e) => this.handleTouchEnd(e), { passive: true });
            document.addEventListener('touchmove', (e) => this.handleTouchCancel(e), { passive: true });

            this.initialized = true;
        },

        handleTouchStart: function(e) {
            const message = e.target.closest('.tb-message');
            if (!message) return;

            this.pressTimer = setTimeout(() => {
                // Trigger long press
                window.vibrate([10, 50, 10]); // Strong haptic feedback
                this.showMessageContextMenu(message, e.touches[0]);
            }, this.threshold);
        },

        handleTouchEnd: function(e) {
            if (this.pressTimer) {
                clearTimeout(this.pressTimer);
                this.pressTimer = null;
            }
        },

        handleTouchCancel: function(e) {
            if (this.pressTimer) {
                clearTimeout(this.pressTimer);
                this.pressTimer = null;
            }
        },

        showMessageContextMenu: function(message, touch) {
            // Dispatch event for Blazor to show context menu
            const event = new CustomEvent('messageContextMenu', {
                detail: {
                    message: message,
                    x: touch.pageX,
                    y: touch.pageY
                }
            });
            window.dispatchEvent(event);
        }
    };

    // ============================================================================
    // 7. SMOOTH SCROLL & MOMENTUM
    // ============================================================================

    /**
     * Enhance scrolling on mobile with momentum and smooth behavior
     */
    function initSmoothScrolling() {
        const scrollContainers = document.querySelectorAll('.tb-chat-messages, .tb-mobile-content, .tb-bottom-sheet');
        scrollContainers.forEach(container => {
            container.style.webkitOverflowScrolling = 'touch'; // iOS momentum scrolling
        });
    }

    // ============================================================================
    // 8. PREVENT ZOOM ON INPUT FOCUS (iOS)
    // ============================================================================

    /**
     * Prevents automatic zoom on input focus (iOS Safari)
     * Ensures font-size is 16px+ on inputs
     */
    function preventAutoZoom() {
        const inputs = document.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            const computedStyle = window.getComputedStyle(input);
            const fontSize = parseFloat(computedStyle.fontSize);

            // iOS Safari zooms if font-size < 16px
            if (fontSize < 16) {
                input.style.fontSize = '16px';
            }
        });
    }

    // ============================================================================
    // 9. SAFE AREA INSET SUPPORT
    // ============================================================================

    /**
     * Dynamically adjust safe area insets for notched devices
     */
    function updateSafeAreaInsets() {
        if (CSS.supports('padding-bottom: env(safe-area-inset-bottom)')) {
            // Browser supports safe-area-inset
            document.body.style.paddingBottom = 'env(safe-area-inset-bottom)';
        }
    }

    // ============================================================================
    // 10. ORIENTATION CHANGE HANDLING
    // ============================================================================

    /**
     * Handle orientation changes gracefully
     */
    function handleOrientationChange() {
        window.addEventListener('orientationchange', () => {
            // Reset viewport height tracking
            KeyboardManager.originalViewportHeight = window.visualViewport ? window.visualViewport.height : window.innerHeight;

            // Dispatch event for components to respond
            setTimeout(() => {
                window.dispatchEvent(new CustomEvent('orientationChanged', {
                    detail: { orientation: screen.orientation?.type || window.orientation }
                }));
            }, 300); // Delay to allow browser to finish orientation change
        });
    }

    // ============================================================================
    // INITIALIZATION
    // ============================================================================

    // Guard flags to prevent duplicate initialization
    let globalInitsComplete = false;
    let keyboardManagerInitialized = false;
    let pullToRefreshInitialized = false;
    let longPressInitialized = false;
    let hapticFeedbackInitialized = false;
    let orientationHandlerInitialized = false;

    /**
     * One-time global initialization (runs only once per page load)
     * Sets up event listeners and managers that should persist
     */
    function initGlobalMobileFeatures() {
        if (globalInitsComplete) {
            console.log('[Mobile Utils] Global initialization already complete, skipping...');
            return;
        }

        const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;

        if (!isMobile && !isTouchDevice) {
            console.log('[Mobile Utils] Not a mobile device, skipping mobile enhancements');
            return;
        }

        console.log('[Mobile Utils] Initializing global mobile enhancements...');

        // Initialize keyboard manager (once)
        if (!keyboardManagerInitialized) {
            KeyboardManager.init();
            keyboardManagerInitialized = true;
        }

        // Initialize pull-to-refresh (once)
        if (!pullToRefreshInitialized) {
            PullToRefresh.init();
            pullToRefreshInitialized = true;
        }

        // Initialize long-press detector (once)
        if (!longPressInitialized) {
            LongPressDetector.init();
            longPressInitialized = true;
        }

        // Initialize haptic feedback (once)
        if (!hapticFeedbackInitialized) {
            initHapticFeedback();
            hapticFeedbackInitialized = true;
        }

        // Initialize orientation change handler (once)
        if (!orientationHandlerInitialized) {
            handleOrientationChange();
            orientationHandlerInitialized = true;
        }

        // One-time setups
        updateSafeAreaInsets();

        globalInitsComplete = true;
        console.log('[Mobile Utils] ✓ Global mobile enhancements initialized');
    }

    /**
     * Per-page initialization (runs on every navigation)
     * Wires up page-specific elements that may have been recreated
     */
    function initPerPageMobileFeatures() {
        const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;

        if (!isMobile && !isTouchDevice) {
            return;
        }

        console.log('[Mobile Utils] Initializing per-page mobile features...');

        // Re-wire textareas (they may be new after navigation)
        initAutoResizeTextareas();

        // Re-apply smooth scrolling to new containers
        initSmoothScrolling();

        // Re-apply auto-zoom prevention to new inputs
        preventAutoZoom();

        // Enable camera capture for new file inputs
        window.enableCameraCapture();

        console.log('[Mobile Utils] ✓ Per-page features initialized');
    }

    /**
     * Main initialization function
     */
    function initMobileUtils() {
        // Always run global inits first (they're guarded internally)
        initGlobalMobileFeatures();

        // Then run per-page inits
        initPerPageMobileFeatures();
    }

    // Initialize when DOM is fully loaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initMobileUtils, { once: true });
    } else {
        initMobileUtils();
    }

    // Re-initialize per-page features on Blazor navigation
    // Global features are already initialized and won't re-run
    window.addEventListener('blazor:enhancednavigation', initPerPageMobileFeatures);

    // Expose utilities globally
    window.tbMobile = {
        KeyboardManager,
        PullToRefresh,
        LongPressDetector,
        autoResizeTextarea: window.autoResizeTextarea,
        vibrate: window.vibrate,
        enableCameraCapture: window.enableCameraCapture,
        showFileUploadOptions: window.showFileUploadOptions
    };

})();
