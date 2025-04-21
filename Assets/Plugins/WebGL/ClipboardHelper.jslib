mergeInto(LibraryManager.library, {
    CopyTextToClipboard: function(ptr) {
      // turn the incoming pointer into a JS string
      /*var text = UTF8ToString(ptr);
  
      // copy‐event listener injects our text into the clipboard
      function onCopy(e) {
        e.clipboardData.setData('text/plain', text);
        e.preventDefault();
      }
  
      console.log("CLIPBOARD: Exec command");

      // register listener, fire the copy command, then remove listener
      document.addEventListener('copy', onCopy);
      document.execCommand('copy');
      document.removeEventListener('copy', onCopy);*/
    }
  });
  