using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



public enum SituationState {Unstarted,FreshlyStarted, Ongoing, RequiringExecution, Complete }
//lifecycle:
//unstarted: no recipe running
//freshly started: recipe running, not yet ticked
//ongoing: timer ticking
//requiringexecution: timer complete, not yet executed
//complete: timer complete, executed, not yet removed outputs
