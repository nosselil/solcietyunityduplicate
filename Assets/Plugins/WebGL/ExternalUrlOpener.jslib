mergeInto(LibraryManager.library, {
    OpenExternalUrl: function(urlPtr) {
        // Debug log to confirm that the jslib function is called
        console.log("DEBUG: OpenExternalUrl called in jslib");

        // Convert the C# string pointer to a JavaScript string
        const url = UTF8ToString(urlPtr);
        console.log("DEBUG: Converted URL from Unity:", url);

        // Ensure the global OpenExternalUrl function is available
        if (typeof window.OpenExternalUrl !== 'function') {
            console.error('DEBUG: OpenExternalUrl is not defined in the page context.');
            return;
        }

        // Call the global OpenExternalUrl function
        console.log("DEBUG: Calling OpenExternalUrl from jslib with URL:", url);
        window.OpenExternalUrl(url);
    }
});
