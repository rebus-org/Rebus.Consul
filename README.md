# Rebus.Consul

[![install from nuget](https://img.shields.io/nuget/v/Rebus.Consul.svg?style=flat-square)](https://www.nuget.org/packages/Rebus.Consul)

Provides a [Consul](https://www.consul.io/)-backed subscription storage for [Rebus](https://github.com/rebus-org/Rebus).

![](https://raw.githubusercontent.com/rebus-org/Rebus/master/artwork/little_rebusbus2_copy-200x200.png)

---

## Integration testing

Download the appropriate release of Consul from [here](https://www.consul.io/downloads.html).

On Windows, a development instance can be fired up by invoking:

```
.\consul.exe agent -dev -ui
```

The Management UI will be available at http://localhost:8500/ui

