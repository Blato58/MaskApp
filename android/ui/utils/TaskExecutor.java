package cn.com.heaton.shiningmask.ui.utils;

import android.os.AsyncTask;
import android.os.Handler;
import android.os.Looper;
import java.util.concurrent.Executor;

/* JADX INFO: loaded from: classes.dex */
public class TaskExecutor {
    private static Executor mParallelExecutor = AsyncTask.THREAD_POOL_EXECUTOR;
    private static Executor mSerialExecutor = AsyncTask.SERIAL_EXECUTOR;
    private static Handler sHandler = new Handler(Looper.getMainLooper());

    private TaskExecutor() {
        mParallelExecutor = AsyncTask.THREAD_POOL_EXECUTOR;
        mSerialExecutor = AsyncTask.SERIAL_EXECUTOR;
    }

    public static void runOnUIThread(Runnable runnable) {
        sHandler.post(runnable);
    }

    public static void runOnUIThreadDelay(Runnable runnable, long j) {
        sHandler.postDelayed(runnable, j);
    }

    public static void executeTask(Runnable runnable) {
        executeTask(runnable, true);
    }

    public static void executeTaskSerially(Runnable runnable) {
        executeTask(runnable, false);
    }

    public static void executeTask(Runnable runnable, boolean z) {
        if (z) {
            mParallelExecutor.execute(runnable);
        } else {
            mSerialExecutor.execute(runnable);
        }
    }
}