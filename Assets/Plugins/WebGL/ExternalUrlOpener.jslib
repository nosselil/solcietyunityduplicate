// Assets/Plugins/WebGL/OpenExternalUrlPlugin.jslib

mergeInto(LibraryManager.library, {
    // This is the function Unity will DllImport
    OpenExternalUrl: function(urlPtr) {
      // Debug to prove we got called
      console.log("DEBUG (JSLib): OpenExternalUrl called");
  
      // Turn the C# string pointer into a JS string
      var url = UTF8ToString(urlPtr);
      console.log("DEBUG (JSLib): Decoded URL:", url);
  
      // Look up the user‐registered handler on the real global object
      var fn = globalThis.OpenExternalUrl;
      if (typeof fn !== "function") {
        console.error("DEBUG (JSLib): globalThis.OpenExternalUrl is not a function");
        return;
      }
  
      // Call it with your URL
      console.log("DEBUG (JSLib): Invoking globalThis.OpenExternalUrl");
      try {
        fn(url);
      } catch (e) {
        console.error("DEBUG (JSLib): Error calling OpenExternalUrl:", e);
      }
    }
  });
  