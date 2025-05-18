// Assets/Plugins/WebGL/GoogleApiPlugin.jslib

mergeInto(LibraryManager.library, {
  // Calls globalThis.GoogleApiController.login()
  GoogleApiLogin: function () {
    console.log("DEBUG (JSLib): GoogleApiLogin called");

    if (typeof GoogleApiController === "undefined" ||
        typeof GoogleApiController.login !== "function")
    {
      console.error("DEBUG (JSLib): GoogleApiController.login is not available");
      return;
    }
    var ctrl = globalThis.GoogleApiController;
    // login() returns a Promise that resolves when "oauth_success" is posted
    GoogleApiController.login()
      .then(function () {
        // use the same global controller reference
        var unity = ctrl.unityInstance;
        if (unity && typeof unity.SendMessage === "function") {
          unity.SendMessage("ProjectorSet", "OnAuthSuccess", "");
          console.log("DEBUG (JSLib): Sent OnAuthSuccess to ProjectorSet");
        } else {
          console.warn("DEBUG (JSLib): unityInstance not assigned or SendMessage missing");
        }
      })
      .catch(err =>
        console.error("DEBUG (JSLib): login() promise rejected:", err)
      );
  },

  // Calls globalThis.GoogleApiController.listSlides(presentationId)
  GoogleApiListSlides: function (presentationIdPtr) {
    console.log("DEBUG (JSLib): GoogleApiListSlides called");

    var ctrl = globalThis.GoogleApiController;              // ES5 – no ?.
    if (!ctrl || typeof ctrl.listSlides !== "function") {
      console.error("DEBUG (JSLib): GoogleApiController.listSlides is not a function");
      return;
    }

    var presentationId = UTF8ToString(presentationIdPtr);
    console.log("DEBUG (JSLib): presentationId =", presentationId);

    try {
      ctrl.listSlides(presentationId)
        .then(function (ids) {                              // ES5 function
          console.log("DEBUG (JSLib): listSlides returned IDs:", ids);

          var msg = JSON.stringify(ids);
          if (ctrl.unityInstance && typeof ctrl.unityInstance.SendMessage === "function") {
            ctrl.unityInstance.SendMessage("ProjectorSet", "OnSlidesListed", msg);
            console.log("DEBUG (JSLib): Sent OnSlidesListed to ProjectorSet");
          } else {
            console.warn("DEBUG (JSLib): unityInstance.SendMessage not available");
          }
        })
        .catch(function (err) {
          console.error("DEBUG (JSLib): listSlides error:", err);
        });
    } catch (e) {
      console.error("DEBUG (JSLib): Exception calling listSlides:", e);
    }
  },

  // Calls globalThis.GoogleApiController.getThumbnailUrl(presentationId, pageId)
  GoogleApiGetThumbnailUrl: function (presentationIdPtr, pageIdPtr) {
    console.log("DEBUG (JSLib): GoogleApiGetThumbnailUrl called");

    var ctrl = globalThis.GoogleApiController;
    if (!ctrl || typeof ctrl.getThumbnailUrl !== "function") {
      console.error("DEBUG (JSLib): GoogleApiController.getThumbnailUrl is not a function");
      return;
    }

    var presentationId = UTF8ToString(presentationIdPtr);
    var pageId         = UTF8ToString(pageIdPtr);
    console.log("DEBUG (JSLib): presentationId =", presentationId, "pageId =", pageId);

    try {
      ctrl.getThumbnailUrl(presentationId, pageId)
        .then(function (url) {
          console.log("DEBUG (JSLib): getThumbnailUrl returned URL:", url);

          // Send back to Unity
          if (ctrl.unityInstance && typeof ctrl.unityInstance.SendMessage === "function") {
            ctrl.unityInstance.SendMessage("ProjectorSet", "OnThumbnailUrlReceived", pageId + "|" + url);
            console.log("DEBUG (JSLib): Sent OnThumbnailUrlReceived to ProjectorSet");
          } else {
            console.warn("DEBUG (JSLib): unityInstance.SendMessage not available");
          }
        })
        .catch(function (err) {
          console.error("DEBUG (JSLib): getThumbnailUrl error:", err);
        });
    } catch (e) {
      console.error("DEBUG (JSLib): Exception calling getThumbnailUrl:", e);
    }
  }
});
