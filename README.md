print
=======

- print is a way of printing debug text onto a Unity Screen.
- Only single file less than 250 lines of code.
- No external assets required.
- Easy to integrate.
- print is MIT licensed.

### Example Usage

Attach `Print.cs` to a new GameObject. Unity will automatically attach a Camera to it.

```c#
Print.Clear();
int mana = 200;
Print.Text(10, 10, "I have Mana");
Print.Format(10, 100, "Mana: {0}", mana);
Print.Value(200, 120, mana);
```

### Notes
- Text is retained on the screen until `Print.Clear()` is called. 
- `x` and `y` are in pixels.
- The camera is set to depth 200, and not to clear the frame.

### Other API
- `FSM.Scale` set the scale of the text

## Releases
- v1.0.0 (09/01/2016)
  - Initial Release