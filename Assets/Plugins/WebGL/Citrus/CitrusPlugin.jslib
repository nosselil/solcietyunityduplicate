mergeInto(LibraryManager.library, {
  // Citrus_BuildBorrowTx(loanAccount, nftMint, borrower)
  Citrus_BuildBorrowTx: function (loanAccountPtr, nftMintPtr, borrowerPtr) {
    try {
      var loanAccount = UTF8ToString(loanAccountPtr);
      var nftMint     = UTF8ToString(nftMintPtr);
      var borrower    = UTF8ToString(borrowerPtr);

      var ctrl = (typeof globalThis !== "undefined" && (globalThis.CitrusController || globalThis.CitrusBridge))
        ? (globalThis.CitrusController || globalThis.CitrusBridge)
        : null;

      if (!ctrl || typeof ctrl.buildBorrowTx !== "function") {
        console.error("[CitrusPlugin] CitrusController.buildBorrowTx is not available on globalThis");
        return;
      }

      var unity = ctrl.unityInstance || (typeof unityInstance !== "undefined" ? unityInstance : null);

      ctrl.buildBorrowTx(loanAccount, nftMint, borrower)
        .then(function (result) {
          try {
            var msg = JSON.stringify(result || {});
            if (unity && typeof unity.SendMessage === "function") {
              // Send to a GameObject named "CitrusManager"
              unity.SendMessage("CitrusClientLoanManager", "OnBorrowBuildSuccess", msg);
            } else {
              console.warn("[CitrusPlugin] unityInstance not set; cannot SendMessage OnBorrowBuildSuccess");
            }
          } catch (jsonErr) {
            console.error("[CitrusPlugin] Failed to stringify result:", jsonErr);
            if (unity && typeof unity.SendMessage === "function") {
              unity.SendMessage("CitrusClientLoanManager", "OnBorrowBuildError", "JSON stringify failed");
            }
          }
        })
        .catch(function (err) {
          var errMsg = (err && err.message) ? err.message : String(err);
          console.error("[CitrusPlugin] buildBorrowTx error:", errMsg);
          if (unity && typeof unity.SendMessage === "function") {
            unity.SendMessage("CitrusClientLoanManager", "OnBorrowBuildError", errMsg);
          }
        });
    } catch (e) {
      console.error("[CitrusPlugin] Exception in Citrus_BuildBorrowTx:", e);
      try {
        var unity = (globalThis.CitrusController && globalThis.CitrusController.unityInstance) ||
                    (typeof unityInstance !== "undefined" ? unityInstance : null);
        if (unity && typeof unity.SendMessage === "function") {
          unity.SendMessage("CitrusClientLoanManager", "OnBorrowBuildError", "Bridge exception: " + String(e));
        }
      } catch (_) { /* no-op */ }
    }
  }
});