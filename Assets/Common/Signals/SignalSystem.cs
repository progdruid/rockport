using System.Collections.Generic;
using ChapterEditor;

namespace Common
{
public class SignalSystem
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private readonly Dictionary<DependencyPath, SignalEmitter> _emitters = new();
    private readonly Dictionary<DependencyPath, SignalListener> _listeners = new();

}

}