# mediocre

Turn your Xiaomi Yeelight into an ambient light by synchronizing it with the average color of your screen.

## Know problems

### Average color is always black/dark when watching Netflix

Netflix' DRM protection might cause screenshots to only contain a black rectangle in place of the video image. This prevents mediocre from calculating the correct average color. The only solution I know is to use Netflix with Chrome or Firefox.

**Not** working:

* Netflix Windows 10 Store App
* Netflix in Internet Explorer, Edge, or Safari

Working:

* Netflix in Chrome or Firefox

## TODO

Basically everything is still work in progress. This is what's planned:

- select monitor
- select device (select all devices by default)
- select application instead of screen as capture surface
- send avg color to stdout (in configurable formats) to use mediocre with other devices
- read colors from stdin (in different formats) to use mediocre with other tools that generate colors
- list available devices
- list available screens
- utilities like turn on/off, set color/brightness
- verify BitBlt() does not need to convert the color format (minimize runtime costs)

## Credits

- https://github.com/roddone/YeelightAPI
- https://github.com/commandlineparser/commandline
