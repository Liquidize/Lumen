
# Lumen

Lumen is a project based on [NightDriverServer](https://github.com/PlummersSoftwareLLC/NightDriverServer), which is designed to send color data over WiFi to an ESP32 running the [NightDriverStrip](https://github.com/PlummersSoftwareLLC/NightDriverStrip) firmware with LEDSTRIP configuration. The ESP32 connects to WiFi and listens for TCP connections on port 49152.

## Main Differences from NightDriverStripServer:

   - Lumen uses JSON to load Location data, allowing for hot reloading and easy addition of locations without recompiling.
   - Lumen features a RESTful API (still WIP) for interacting with the server.
   - Lumen is separated into "Lumen" which is the main functionality, and "Lumen.Api" which allows third party development of different types of effects and canvases that can be distributed as their own projects or dll's.
   - Lumen abstracts its drawing functionality from the "Location" class into "Canvas" classes, enabling custom canvases to be loaded and assigned to locations.
   
   
## About the Code
Quite a bit of the code is directly inspired or even taken from NightDriverServer itself. This project was partially created as a way for me to familiarize myself with the functionality of NightDriverServer via writing the code again instead of just reading it. For that reason this project will be licensed under whatever license NightDriverServer is using, which I believe to be GPL 2.

Some aspects are rather different due to my intention of having this be used by some end users without having to compile their own version, thus an attempt to keep it relatively simple. In addition to not really understanding the purpose as to why they were done that way in NightDriverServer to begin with.  Some examples of this are as follows:

- Locations are a single class and are not children of a base class, so JSON creation of them is easier to deal with.
- There is only a single "LedControllerChannel" class vs the base class and then "LightStrip" child. from NightDriverServer.  This class is called "ControllerChannel".  I personally am not really sure what the benefit of making it extensible is, all of the examples within NightDriverServer only use the LightStrip implementation, and its purpose really is just to queue up the color byte array to send to the ESP module. The only issues I personally see with this is currently the implementation uses Dave's logic for sending the data to the strip a few seconds in advance, and not right away. This will likely change in the future to make it some form of setting in the JSON. Personally I don't see why you'd want to do that though, because my testing showed that the strip would sometimes skip around frames with that.
- CRGB color class is known as "LedColor" instead, this was a change I made mainly for myself as I felt the name was more descriptive, probably a highly controversial choice, and I'm well aware it is named for the CRGB class in FastLED. Changing it may make it a bit confusing for some when switching between working in the C++ NightDriverStrip and this project but ideally the purpose of this project is to abstract having to even touch the firmware, and just treat the esp as some sort of "translator" to just show effects from this.
- Stats are not visible for each strip, I have plans on potentially creating some form of React based dashboard to view them and set effects from but that will be some time.

With that being said this project is mainly being used as a learning opportunity for me for interacting with ESP, React, ASP .Net, etc so I'm rather open for feedback and insight.

## Future Plans / TODO


- "Complete" the REST API
- Add more default effect types
- Add a reactjs dashboard to make interactions easier for end users
