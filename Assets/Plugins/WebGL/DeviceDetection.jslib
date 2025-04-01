var DeviceDetection = {
    // Function to detect mobile devices
    IsMobile: function() {
        var userAgent = navigator.userAgent || navigator.vendor || window.opera;
        // Regular expressions to identify mobile devices
        var isMobile = /android/i.test(userAgent) ||
                       /iPad|iPhone|iPod/.test(userAgent) && !window.MSStream;
        return isMobile;
    },
    
    // Function to lock the orientation to landscape
    LockOrientationToLandscape: function() {
        // Check if the page is already in fullscreen mode; if not, request fullscreen
        if (!document.fullscreenElement && !document.mozFullScreenElement && !document.webkitFullscreenElement && !document.msFullscreenElement) {
            // Request fullscreen mode on user interaction
            if (document.documentElement.requestFullscreen) {
                document.documentElement.requestFullscreen();
            } else if (document.documentElement.mozRequestFullScreen) { // Firefox
                document.documentElement.mozRequestFullScreen();
            } else if (document.documentElement.webkitRequestFullscreen) { // Chrome, Safari
                document.documentElement.webkitRequestFullscreen();
            } else if (document.documentElement.msRequestFullscreen) { // IE/Edge
                document.documentElement.msRequestFullscreen();
            }
        }

        // For Safari on iOS, use webkitRequestFullscreen
        if (document.documentElement.webkitRequestFullscreen) {
            document.documentElement.webkitRequestFullscreen();
        }

        // Once fullscreen is triggered, lock orientation to landscape
        if (screen.orientation && screen.orientation.lock) {
            screen.orientation.lock('landscape').catch(function (error) {
                console.error('Orientation lock failed:', error);
            });
        } else {
            console.warn('Screen orientation API not supported.');
        }
    }
};

// Ensure the plugin is loaded before Unity starts
mergeInto(LibraryManager.library, DeviceDetection);
