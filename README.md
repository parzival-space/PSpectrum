# PSpectrum Audio Visualizer

PSpectrum allows programs that can not access the audio devices of your system, to access the audio data of the system. <br>
I mainly use it for my <a href="https://github.com/malte-linke/powercord-pspectrum">PSpectrum Audio Visualizer</a> for Powercord. <br>
You can use the `web` subcommand to start a demo audio visualizer in your browser. <br>
To capture the system audio I use the Bass library made by <a href="https://www.un4seen.com/">un4seen</a>.

## Commands

The most interesting commands are `web`, `listen` and `devices`.<br>

| Command   | Description                                           |
| :-------- | :---------------------------------------------------- |
| `web`     | Starts a demo audio visualizer in your browser.       |
| `listen`  | Prints the current audio data into the output stream. |
| `devices` | Prints the available audio devices.                   |
| `help`    | Prints the help page.                                 |
| `version` | Prints the version of the program.                    |

Nearly all commands have a bunch of options. You can find them by using the `--help` option.<br>
For example: `pspectrum web --help`.

## How to use

Using PSpectrum should be quite easy. <br>
By using the `listen` command, PSpectrum will print the audio data into the output stream. <br>
The data that gets printed, is an array of floats containing the audio data of the left and right channel. <br>
You can change the size of the data by using the `--buffer-size` option. <br>
<br>
For example: `pspectrum listen --buffer-size=1024` <br>
This will return float arrays of size 1024. <br>
The first 512 floats represent the left channel, the next 512 floats the right channel. <br>

<details>
  <summary>Node.js Example</summary>

```js
const { spawn } = require('child_process');

// spawn PSpectrum
var pspectrum = spawn('PSpectrum.exe', ['listen']);

// process the output
pspectrum.stdout.on('data', (line) => {
  let data = JSON.parse(line);               // this will contain the left and right channel
  let left = data.slice(0, data.length / 2); // left channel
  let right = data.slice(data.length / 2);   // right channel

  // do something with the data
  ...
});
```

</details>

## v1 vs v2

PSpectrum v2 is basically a complete rewrite of the program. It is more stable and has a lot of new features. <br>
The normalization of the audio data has been greatly improved. In v1 the audio data had a maximum of 2 normalization levels. v2 allows infinite normalization levels. <br>
As I mentioned earlier, v2 also has a demo visualizer running in your browser. For the moments when you want to do something like <a href="https://i.imgur.com/my2ZJlF.mp4">this</a>.

## Demos

<details>
  <summary>Web Command</summary>

  ```cmd
  PSpectrum.exe web -b 512
  ```
  <img src="https://i.imgur.com/LKjcoxS.gif">
</details>

<details>
  <summary>Listen Command</summary>

  ```cmd
  PSpectrum.exe listen -b 64
  ```
  <img src="https://i.imgur.com/KK3yjXr.gif">
</details>

<details>
  <summary>Devices Command</summary>
  
  ```cmd
  PSpectrum.exe devices --output
  PSpectrum.exe devices --input
  ```
  <img src="https://i.imgur.com/Y8vaW57.gif">
</details>
