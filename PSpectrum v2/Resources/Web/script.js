/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2021 malte-linke
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


(() => {
  // the rest of the code shouldn't be accessible from the outside (especially for the custom scripts)
  document.body.addEventListener("click", () => {
    ToggleFullscreen();
    UpdateSizes();
  });
  window.addEventListener("resize", UpdateSizes);

  // Run first-launch code
  UpdateSizes();
  LoadCustomScript($CUSTOM_SCRIPT$);

  // start socket connection
  var connection = SocketClient($SOCKET_PORT$);
  connection.onData(d => DrawFrame(d));
  connection.onError(console.log);
  connection.onClose(() => DrawFrame([]));
  
  // first frame
  DrawLoading();

  // make connection public
  window.socket = connection;

  function DrawFrame(data = []) {
    var render = document.querySelector('canvas');
    var ctx = render.getContext('2d');

    // render line
    var line = (x1, y1, x2, y2, color = '#FFF') => {
      ctx.beginPath();
      ctx.moveTo(x1, y1);
      ctx.lineTo(x2, y2);
      ctx.strokeStyle = color;
      ctx.stroke();
    };

    // render rect
    var rect = (x, y, w, h, color = '#FFF') => {
      ctx.beginPath();
      ctx.rect(x, y, w, h);
      ctx.fillStyle = color;
      ctx.fill();
    };

    // clear previous frame
    ctx.clearRect(0, 0, render.width, render.height);

    // calculate bar sizes
    var barWidth = render.width / data.length;
    var barHeight = render.height / 2;

    //line(render.width / 2, 0, render.width / 2, render.height, 'lime');
    //line(0, render.height / 2, render.width, render.height / 2, 'lime');

    // split data into left and right channel
    var left = data.slice(0, data.length / 2);
    var right = data.slice(data.length / 2);

    // to center the frequencys, one needs to be reversed
    left.reverse();

    // draw left channel
    for (var i = 0; i < left.length; i++) {
      var x = i * barWidth;
      var y = render.height / 2;
      var height = left[i] * barHeight;
      rect(x, y, barWidth, -height, 'blue'); // top left
      rect(x, y, barWidth, height, 'rgba(0, 0, 155, 0.5)');
    }

    // draw right channel
    for (var i = 0; i < right.length; i++) {
      var x = i * barWidth + render.width / 2;
      var y = render.height / 2;
      var height = right[i] * barHeight;
      rect(x, y, barWidth, -height, 'red'); // top right
      rect(x, y, barWidth, height, 'rgba(155, 0, 0, 0.5)');
    }
  };

  /**
   * Toggle Fullscreen
   */
  function ToggleFullscreen() {
    var isInFullScreen = (document.fullScreenElement && document.fullScreenElement !== null) || (document.mozFullScreen || document.webkitIsFullScreen);
    var elem = document.documentElement;

    if (isInFullScreen) {
      if (document.exitFullscreen) {
        document.exitFullscreen();
      } else if (document.webkitExitFullscreen) {
        /* Safari */
        document.webkitExitFullscreen();
      } else if (document.msExitFullscreen) {
        /* IE11 */
        document.msExitFullscreen();
      } else if (document.mozCancelFullScreen) {
        /* Firefox */
        document.mozCancelFullScreen();
      }
    } else {
      if (elem.requestFullscreen) {
        elem.requestFullscreen();
      } else if (elem.webkitRequestFullscreen) {
        /* Safari */
        elem.webkitRequestFullscreen();
      } else if (elem.msRequestFullscreen) {
        /* IE11 */
        elem.msRequestFullscreen();
      } else if (elem.mozRequestFullScreen) {
        /* Firefox */
        elem.mozRequestFullScreen();
      }
    }
  }

  /**
   * Update Sizes
   */
  function UpdateSizes() {
    var width = document.documentElement.clientWidth;
    var height = document.documentElement.clientHeight;

    var render = document.querySelector('canvas');

    render.setAttribute("width", width);
    render.setAttribute("height", height);
  }

  /**
   * Socket Client
   */
  function SocketClient(port, ip = '127.0.0.1', reconnect = 500) {
    let state = false;

    let registerCallbacks = () => {
      state: state,

      // redirect events
      socket.onopen = () => callbacks.ready.forEach(c => c());
      socket.onmessage = (m) => callbacks.data.forEach(c => c(JSON.parse(m.data)));
      socket.onerror = (e) => callbacks.error.forEach(c => c(e));
      socket.onclose = () => {
        callbacks.close.forEach(c => c());

        // try to reconnect if connection gets closed
        setTimeout(connect, reconnect);
      };
    };

    // reconnect if the connection takes too long
    function connect() {
      if (state) return;
      socket = new WebSocket(`ws://${ip}:${port}`);
      registerCallbacks();

      setTimeout(connect, 2000);
    }

    let callbacks = {
      ready: [],
      data: [],
      close: [],
      error: []
    };

    callbacks.close.push(() => state = false);
    callbacks.ready.push(() => state = true);

    // connect on init
    connect();

    return {
      onData: (cb) => callbacks.data.push(cb),
      onReady: (cb) => callbacks.ready.push(cb),
      onClose: (cb) => callbacks.close.push(cb),
      onError: (cb) => callbacks.error.push(cb),
    }
  }

  /**
   * Custom Script
   */
  function LoadCustomScript(enabled) {
    if (!enabled) return;

    let script = document.createElement("script");
    script.src = "custom.js";
    script.defer = true;
    script.type = "text/javascript";
    document.head.appendChild(script);
  }

  /**
   * Draw Loading Message
   */
  function DrawLoading() {
    if (connection.state) return;
  
    // get canvas
    var render = document.querySelector('canvas');
    var ctx = render.getContext('2d');
  
    // print text in buttom left corner
    ctx.font = "20px Arial";
    ctx.fillStyle = "white";
    ctx.fillText("Waiting for connection...", 5, render.height - 5);
  };
})();