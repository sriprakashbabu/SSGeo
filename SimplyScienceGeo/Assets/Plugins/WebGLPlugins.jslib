mergeInto(LibraryManager.library, {
  // This function checks if the browser reports being on a touch-first device.
  IsMobileDevice: function () {
    return ('ontouchstart' in window) || (navigator.maxTouchPoints > 0);
  },

  RequestFullscreenAndLockLandscape: function () {
    const canvas = document.querySelector("#unity-canvas");
    if (canvas.requestFullscreen) {
      canvas.requestFullscreen()
        .then(function() {
          screen.orientation.lock('landscape').catch(function(error) {
            console.log("Orientation lock failed:", error);
          });
        })
        .catch(function(error) {
          console.log("Fullscreen request failed:", error);
        });
    } else {
        console.log("Fullscreen API is not supported.");
    }
  },
});