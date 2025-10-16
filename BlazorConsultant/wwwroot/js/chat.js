/**
 * TailorBlend AI Consultant - JavaScript Helpers
 *
 * Utility functions for Blazor UI enhancements.
 */

/**
 * Scroll to bottom of messages container.
 * Called after each message or token to keep latest content visible.
 *
 * @param {HTMLElement} element - The messages container element
 */
window.scrollToBottom = function (element) {
    if (element) {
        element.scrollTo({
            top: element.scrollHeight,
            behavior: 'smooth'
        });
    }
};

/**
 * Focus input element.
 *
 * @param {HTMLElement} element - The input element to focus
 */
window.focusElement = function (element) {
    if (element) {
        element.focus();
    }
};

/**
 * Copy text to clipboard.
 *
 * @param {string} text - Text to copy
 * @returns {Promise<boolean>} - Success status
 */
window.copyToClipboard = async function (text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy:', err);
        return false;
    }
};
