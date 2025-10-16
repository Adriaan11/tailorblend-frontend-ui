/**
 * File utility functions for file attachment support.
 * Handles reading files to base64 and MIME type detection.
 */

/**
 * Read a file from an input element and convert to base64.
 * @param {HTMLInputElement} inputElement - The file input element
 * @param {number} maxFileSizeMB - Maximum file size in MB (default: 10)
 * @returns {Promise<Array>} Array of file objects with {filename, base64_data, mime_type, file_size}
 */
window.readFilesAsBase64 = async function (inputElement, maxFileSizeMB = 10) {
    if (!inputElement || !inputElement.files || inputElement.files.length === 0) {
        return [];
    }

    const maxSizeBytes = maxFileSizeMB * 1024 * 1024;
    const files = Array.from(inputElement.files);
    const results = [];

    for (const file of files) {
        // Check file size
        if (file.size > maxSizeBytes) {
            throw new Error(`File "${file.name}" exceeds maximum size of ${maxFileSizeMB}MB (${formatFileSize(file.size)} provided)`);
        }

        // Read file as base64
        const base64 = await readFileAsBase64(file);

        // Use snake_case to match C# JsonPropertyName attributes
        results.push({
            filename: file.name,
            base64_data: base64,
            mime_type: file.type || detectMimeType(file.name),
            file_size: file.size
        });
    }

    return results;
};

/**
 * Read a single file and convert to base64.
 * @param {File} file - The file to read
 * @returns {Promise<string>} Base64 encoded file content (without data URI prefix)
 */
function readFileAsBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();

        reader.onload = () => {
            // Remove the data URI prefix (e.g., "data:image/png;base64,")
            const base64 = reader.result.split(',')[1];
            resolve(base64);
        };

        reader.onerror = (error) => {
            reject(new Error(`Failed to read file "${file.name}": ${error}`));
        };

        reader.readAsDataURL(file);
    });
}

/**
 * Detect MIME type from filename extension.
 * @param {string} filename - The filename with extension
 * @returns {string} MIME type
 */
function detectMimeType(filename) {
    const ext = filename.split('.').pop().toLowerCase();
    const mimeTypes = {
        'pdf': 'application/pdf',
        'jpg': 'image/jpeg',
        'jpeg': 'image/jpeg',
        'png': 'image/png',
        'gif': 'image/gif',
        'txt': 'text/plain',
        'csv': 'text/csv',
        'xlsx': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'docx': 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
    };
    return mimeTypes[ext] || 'application/octet-stream';
}

/**
 * Format file size for display (e.g., "2.5 MB", "150 KB").
 * @param {number} bytes - File size in bytes
 * @returns {string} Formatted size string
 */
function formatFileSize(bytes) {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/**
 * Clear file input element.
 * @param {HTMLInputElement} inputElement - The file input element to clear
 */
window.clearFileInput = function (inputElement) {
    if (inputElement) {
        inputElement.value = '';
    }
};

/**
 * Get file count from input element.
 * @param {HTMLInputElement} inputElement - The file input element
 * @returns {number} Number of files selected
 */
window.getFileCount = function (inputElement) {
    if (!inputElement || !inputElement.files) {
        return 0;
    }
    return inputElement.files.length;
};
