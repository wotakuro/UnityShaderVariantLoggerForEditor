#include <pthread.h>

// From PlatformDependSource
long GetThreadId(){
    __uint64_t tid;
    pthread_t pthread = pthread_self();
    pthread_threadid_np(pthread,&tid);
    
    return tid;
}
