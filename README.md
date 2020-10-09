# PngToMlog
Converts a png image to mlog commands. Uses a lightweiht vectorization method i wrote to optimize the output. only works on windows unless you use a program like wine to run it on linux.

## Usage
When you first open it a file prompt will open, this is where you choose your image, select the one you want just like any other file prompt

next it will probpt you with a series of questions
```vanilla small screen is 80x80
vanilla big screen is 176x176
Screen width:

Screen height:

Desired image width:

Desired image height:

Instructions per processor, 990 recommended as it ensures you dont run out of space before this finishes:

Image optimization amount, 0 is no optimization, 100 is 100% optimization
```

Screen width and height are the dimensions (in pixels) of the display

Desired image width and height are the dimensions you want the image to appear as on the screen, it will automatically scale the image to the specified size, and upon generation scale each pixel to the optimal size to fill the display as best it can

Instructions per processor is how many instructions you want in each processor, this is an approximate value and will not be exact. if you dont know what im talking about just type 990. putting 990 compensates for any inaccuracies that may come in my program.

Image optimization amount is how much you want the image to be optimized. optimizing will also reduce the image quality (it isnt overly noticeable but it is there) this only works if you download the command line binary of pngquant from https://pngquant.org/. To add it you have to download the **WINDOWS COMMAND LINE BINARY**, then extract the files inside to a folder called "pngquant" in same folder as my exe.

after you have put in the required info, it will start generating, and then copy the first set of commands to your clipboard. paste that in a processor, then come back to the program, press enter, it will generate the next set and put it in your clipboard, you should paste this in another processor. repeat this until the program closes itself. after connecting all of the processors to a display it will show the image.
