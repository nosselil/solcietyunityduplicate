mergeInto(LibraryManager.library, {
  // Calls globalThis.MatricaApiController.login()
  MatricaApiLogin: function () {
    console.log("DEBUG (JSLib): MatricaApiLogin called");

    var ctrl = globalThis.MatricaApiController;
    if (!ctrl || typeof ctrl.login !== "function") {
      console.error("DEBUG (JSLib): MatricaApiController.login is not available");
      return;
    }

    // login() returns a Promise that resolves on 'oauth_success'
    ctrl.login()
      .then(function () {
        var unity = ctrl.unityInstance;
        if (unity && typeof unity.SendMessage === "function") {
          unity.SendMessage("GatedPortal", "OnAuthSuccess", "");
          console.log("DEBUG (JSLib): Sent OnAuthSuccess to ProjectorSet");
        } else {
          console.warn("DEBUG (JSLib): unityInstance not assigned or SendMessage missing");
        }
      })
      .catch(function (err) {
        console.error("DEBUG (JSLib): MatricaApiController.login() rejected:", err);
      });
  },

  // Calls globalThis.MatricaApiController.checkNftOwnership()
  MatricaApiCheckOwnership: function () {
    console.log("DEBUG (JSLib): MatricaApiCheckOwnership called");

    var ctrl = globalThis.MatricaApiController;
    if (!ctrl || typeof ctrl.checkNftOwnership !== "function") {
      console.error("DEBUG (JSLib): MatricaApiController.checkNftOwnership is not available");
      return;
    }

    try {
      ctrl.checkNftOwnership();
    } catch (e) {
      console.error("DEBUG (JSLib): Exception calling checkNftOwnership:", e);
    }
  }
});
