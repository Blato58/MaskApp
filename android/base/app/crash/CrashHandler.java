package cn.com.heaton.shiningmask.base.app.crash;

import android.content.Context;
import android.content.Intent;
import android.os.Handler;
import android.os.Process;
import android.util.Log;
import cn.com.heaton.shiningmask.base.app.UploadManager;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.TaskExecutor;
import java.lang.Thread;

/* JADX INFO: loaded from: classes.dex */
public class CrashHandler implements Thread.UncaughtExceptionHandler {
    public static final String TAG = "CrashHandler";
    private Context context;
    private CrashCollect crashCollect;
    private CrashUploader crashUploader;
    private Thread.UncaughtExceptionHandler defaultHandler;
    private Class targetClass;

    public interface CrashUploader {
        void crashMessage(CrashInfo crashInfo);
    }

    public CrashHandler() {
    }

    private CrashHandler(Builder builder) {
        this.crashUploader = builder.crashUploader;
        this.targetClass = builder.targetClass;
    }

    public void init(Context context) {
        this.context = context;
        this.crashCollect = new CrashCollect(context);
        this.defaultHandler = Thread.getDefaultUncaughtExceptionHandler();
        Thread.setDefaultUncaughtExceptionHandler(this);
        if (CrashCollect.getCrashLogFile().exists()) {
            LogUtil.d("上传上次的错误信息");
            UploadManager.uploadCrashInfo(this.crashCollect.collectCrashInfo(null));
        }
    }

    public static class Builder {
        private CrashUploader crashUploader;
        private Class targetClass;

        public Builder crashUploader(CrashUploader crashUploader) {
            this.crashUploader = crashUploader;
            return this;
        }

        public Builder targetClass(Class cls) {
            this.targetClass = cls;
            return this;
        }

        public CrashHandler build() {
            return new CrashHandler(this);
        }
    }

    @Override // java.lang.Thread.UncaughtExceptionHandler
    public void uncaughtException(final Thread thread, final Throwable th) {
        Thread.UncaughtExceptionHandler uncaughtExceptionHandler;
        if (!catchCrashException(th) && (uncaughtExceptionHandler = this.defaultHandler) != null) {
            uncaughtExceptionHandler.uncaughtException(thread, th);
        } else {
            new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.base.app.crash.CrashHandler.1
                @Override // java.lang.Runnable
                public void run() {
                    Log.e("CrashHandler", "uncaughtException: 终止退出程序");
                    if (CrashHandler.this.targetClass == null) {
                        CrashHandler.this.defaultHandler.uncaughtException(thread, th);
                    }
                    System.exit(0);
                    Process.killProcess(Process.myPid());
                }
            }, 4000L);
        }
    }

    private boolean catchCrashException(final Throwable th) {
        if (th == null) {
            return false;
        }
        toTargetActivity();
        TaskExecutor.executeTask(new Runnable() { // from class: cn.com.heaton.shiningmask.base.app.crash.CrashHandler.2
            @Override // java.lang.Runnable
            public void run() {
                try {
                    Log.e("CrashHandler", "正在上传崩溃信息到服务器..." + CrashHandler.this.crashCollect);
                    CrashInfo crashInfoCollectCrashInfo = CrashHandler.this.crashCollect.collectCrashInfo(th);
                    CrashHandler.this.crashCollect.saveCrashInfoToFile(crashInfoCollectCrashInfo.getExceptionInfo());
                    Thread.sleep(1000L);
                    if (CrashHandler.this.crashUploader != null) {
                        CrashHandler.this.crashUploader.crashMessage(crashInfoCollectCrashInfo);
                    } else {
                        UploadManager.uploadCrashInfo(crashInfoCollectCrashInfo);
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        });
        try {
            Thread.sleep(1500L);
            return true;
        } catch (InterruptedException e) {
            e.printStackTrace();
            return true;
        }
    }

    private void toTargetActivity() {
        if (this.targetClass != null) {
            Intent intent = new Intent();
            intent.setClass(this.context, this.targetClass);
            intent.setFlags(268468224);
            this.context.startActivity(intent);
        }
    }
}