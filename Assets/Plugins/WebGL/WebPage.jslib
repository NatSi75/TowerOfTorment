mergeInto(LibraryManager.library, {
  // Leaves fullscreen when the player quits (see TowerOfTorment.exitFullscreen in the WebGL template).
  WebPageExitFullscreen: function () {
    if (window.TowerOfTorment && window.TowerOfTorment.exitFullscreen) {
      window.TowerOfTorment.exitFullscreen();
      return;
    }
    try {
      var exit = document.exitFullscreen || document.webkitExitFullscreen;
      if (exit && (document.fullscreenElement || document.webkitFullscreenElement)) exit.call(document);
    } catch (e) { }
  }
});
