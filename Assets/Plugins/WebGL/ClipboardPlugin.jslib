// Assets/Plugins/WebGL/ClipboardPlugin.jslib

/*
// Full implementation is commented out for now:

var ClipboardPlugin = {
  text:    "",
  enabled: false,
  inited:  false,

  init: function() {
    // …setup logic…
  },

  prepareOnce: function(ptr) {
    // …arm for next click…
  }
};
*/

mergeInto(LibraryManager.library, {
    // Stub: exported function does nothing
    PrepareCanvasCopy: function(ptr) {
      // Intentionally left blank for now
    }
  });
  