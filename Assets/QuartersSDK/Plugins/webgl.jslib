mergeInto(LibraryManager.library, {

  OpenWebView: function (url) {
    var receiveMessage = function (event) {
      if (!/https:\/\/(\w+\.)?pocketfulofquarters.com/.test(event.origin)) {
        return;
      }

      if (event.data.callback && event.data.url) {
        window.WebViewUrl = event.data.url;
        SendMessage('QuartersWebView', event.data.callback, event.data.url);
      } else {
        if (event.source && event.source.close) {
          event.source.close();
        }

        var data = JSON.parse(event.data);

        if (data.frameId) {
          var frameEl = document.getElementById(data.frameId);
          if (frameEl) {
            document.body.removeChild(frameEl);
            document.body.removeChild(this.WebViewButton);
            SendMessage('QuartersWebView', 'OnWebViewClosed', this.WebViewUrl);
          }
        }

        SendMessage('QuartersWebView', 'OnWebViewReceivedData', event.data);

        window.removeEventListener('message', receiveMessage);
      }
    }

    window.addEventListener('message', receiveMessage, false);

    var frameId = 'quarters_iframe_' + Date.now();
    var iframeURL = Pointer_stringify(url) + '&frame_id=' + frameId;
    var f = document.createElement('iframe');
    f.setAttribute('src', iframeURL);
    f.setAttribute('id', frameId);
    f.setAttribute(
      'style',
      'position: fixed; width: 60%; height: 90%; top: 5%; left: 20%; z-index: 1000; border: 0 none transparent; background-color: white; box-shadow: 0 0 22px 0 black; border-radius: 11px'
    );
    f.setAttribute('frameBorder', '0');
    f.addEventListener('load', function () {
      var b = document.createElement('button');
      b.innerHTML = '<svg aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" style="color: #616971; width: 100%; height: 100%"><path fill="currentColor" d="M256 8C119 8 8 119 8 256s111 248 248 248 248-111 248-248S393 8 256 8zm121.6 313.1c4.7 4.7 4.7 12.3 0 17L338 377.6c-4.7 4.7-12.3 4.7-17 0L256 312l-65.1 65.6c-4.7 4.7-12.3 4.7-17 0L134.4 338c-4.7-4.7-4.7-12.3 0-17l65.6-65-65.6-65.1c-4.7-4.7-4.7-12.3 0-17l39.6-39.6c4.7-4.7 12.3-4.7 17 0l65 65.7 65.1-65.6c4.7-4.7 12.3-4.7 17 0l39.6 39.6c4.7 4.7 4.7 12.3 0 17L312 256l65.6 65.1z"></path></svg>'
      b.setAttribute(
        'style',
        'position: fixed; width: 40px; height: 40px; top: calc(5% - 17px); right: calc(20% - 17px); z-index: 2000; border: 0 none transparent; background-color: #fff; color: #484646; font-size: 18px; border-radius: 50%; padding: 0'
      );
      b.addEventListener('click', function () {
        document.body.removeChild(window.WebViewFrame);
        document.body.removeChild(window.WebViewButton);
        SendMessage('QuartersWebView', 'OnWebViewClosed', window.WebViewUrl);
      }, false);

      window.WebViewButton = b;
      document.body.appendChild(b);

      window.WebViewUrl = iframeURL;
      SendMessage('QuartersWebView', 'OnUrlOpen', iframeURL);
    }, false)

    window.WebViewFrame = f;
    document.body.appendChild(f);
  },

  CloseWebView: function () {
    document.body.removeChild(this.WebViewFrame);
    document.body.removeChild(this.WebViewButton);
    SendMessage('QuartersWebView', 'OnWebViewClosed', this.WebViewUrl);
  }

});
