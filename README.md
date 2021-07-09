# PSpectrum Audio Visualizer

This is the worker process executeable for my <a href="https://github.com/malte-linke/powercord-pspectrum">PSpectrum Audio Visualizer</a> Powercord plugin.<br>
The executeable prints in an interval of 20ms an array with 256 entys to the console. The first 128 entrys are for the left and the remaining 128 entrys are for the right channel.<br>
To capture the system audio I use the Bass library made by <a href="https://www.un4seen.com/">un4seen</a>.

The builds of PSpectrum get automatically pushed to <a href="https://github.com/malte-linke/powercord-pspectrum">malte-linke/powercord-pspectrum</a> using GitHub Actions.