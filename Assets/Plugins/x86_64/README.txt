I removed the libsteam_api.so file in the x86 folder, because the libsteam_api.so plugin binary there collided with the one from here.
It might be better to change settings on the plugins so they go into the correct builds, but I'm not clear
on the combinations of libsteam_api.so for OSX and Linux and the processor flavours are specified in two places.

This has come up for discussion on the Steamworks.NET thread, and there may be an incoming fix.

Alternatively, future upgrades/reinstalls of Steamworks.NET may restore x86/libsteam_api.so, potentially causing headscratching bugs. Unless, future AK, you read this note!