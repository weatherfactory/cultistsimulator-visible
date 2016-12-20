using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



public enum SituationState {Unstarted,FreshlyStarted, Ongoing, RequiringExecution, Complete }
//unstarted: no recipe running
//freshly started: recipe running, not yet hit Complete
//ongoing: timer ticking
//requiringexecution: timer complete, not yet executed
//ending: this may be redundant
//complete: timer complete, executed, not yet removed outputs
