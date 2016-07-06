# Host
Allows simple and quick debugging of functions/processes

The Host contains a HostCommand class which holds the method name and the users arguments.

All methods are loaded into a dictionary that uses the inputed method name in the HostCommand as the key. 

The Host will validate the inputted command against the dictionary and then validate the parameters against the inputted method.

The inputted arguments will be converted to the appropriate type prior to invoking the method. 

Currently Supported:

* Function Overloads (With No Default Parameters)
* Functions with a Single Default Parameter
* Functions with input and default parameters
* Lists are supported.

Contains System and Test functions that demonstrate the abilities of the Host. 
