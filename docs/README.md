# AppFlow registration 
The simplest way to create .NET app flow is using the 'IHostBuilder' extension method 'ArtMind.AppFlow.RegisterAppFlow.

# todo
1. [ ] to do add task name
1. [ ] create uniform logger
5. [x] **Service Options**
   - [x] _Occurrence_ The number of service flow cycles to be executed
   - [x] _Recurrence_ The minimum time interval duration of a service flow cycle, until to start next session.
   - [x] _Scheduler_ Schedule service to start at
6. [x] **App Options**
   - [x] _Postpone_ Postpone application start. Flow will start after the postpone interval will elapse
   - [x] _Scheduler_ Schedule cycles
6. [ ] **Multithreading** - runing multiple services/apps
8. [ ] Flow Extentions
   - [x] if/else brach
   - [ ] Nested workers / middlewares (inception) a->a1'->a''->....
1. [ ] AppTasks
   1. [ ] TryTask 
   1. [ ] Async tasks
10. [ ] IContext as TIn TOut in the chaning of tasks
11. [ ] Multithreading
12. [ ] Application lifecycle
