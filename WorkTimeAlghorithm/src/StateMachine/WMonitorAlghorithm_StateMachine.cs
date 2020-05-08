using System;
using Serilog;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
{
    public partial class WMonitorAlghorithm
    {
        internal enum Triggers
        {
            Start, NoFace, FaceNotRecog, FaceRecog, MouseMv, KeyboardMv, Pause, Resume, ManualTrigger, ManualCancel, Stop
        }

        internal enum States
        {
            s1,s2,s3,s5,MANUAL, PAUSE_STATE, STOP_STATE
        }

        internal class State
        {
            public bool CanCapureMouseKeyboard;
        }

        private StateMachine<Triggers, States> _sm;
#if DEV_MODE
        private StateMachineVis<Triggers, States> _vis;
#endif
        private readonly State _state = new State();

        private void BuildStateMachine()
        {
            _sm = new StateMachineBuilder<Triggers, States>()
                .CreateState(States.s1)
                .Ignoring()
                .Transition(Triggers.Start, States.s2)
                .End()

                .CreateState(States.s2)
                .EnterAsync((t) => _state2.Enter(_state, _sm, _workTime, this))
                .Transition(Triggers.FaceRecog, States.s5)
                .Transition(Triggers.FaceNotRecog, States.s3)
                .Transition(Triggers.NoFace, States.s3)
                .Transition(Triggers.ManualTrigger, States.MANUAL)
                .Ignoring()
                .End()

                .CreateState(States.s3)
                .Ignoring()
                .EnterAsync(t => _state3.Enter(_state, _sm, _workTime, this))
                .Transition(Triggers.FaceRecog, States.s5)
                .Transition(Triggers.ManualTrigger, States.MANUAL)
                .End()

                .CreateState(States.s5)
                .Ignoring()
                .Transition(Triggers.FaceNotRecog, States.s2)
                .EnterAsync(t => _state5.Enter(_state, _sm))
                .End()

                .CreateState(States.MANUAL)
                .Ignoring()
                .Enter(_ => { })
                .Transition(Triggers.FaceRecog, States.s5)
                .Transition(Triggers.ManualCancel, States.s3)
                .End()

                .HoldingGlobState(Triggers.Pause, _ => Log.Logger.Debug("PAUSE"), 
                    States.PAUSE_STATE, Triggers.Resume)


                .HoldingGlobState(Triggers.Stop, _ => Log.Logger.Debug("STOP"), States.STOP_STATE, Triggers.Stop)

                .Build(States.s1);
        }


        private void InitStateMachine()
        {
            BuildStateMachine();
#if DEV_MODE
            _vis = new StateMachineVis<Triggers, States>(_sm, pipeName: "graphViz", loggingEnabled: false);
            _vis.Start(@"StateMachineLibVis.exe", "-c graphViz -l 970");
#endif
        }

    }
}