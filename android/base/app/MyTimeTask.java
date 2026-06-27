package cn.com.heaton.shiningmask.base.app;

import java.util.Timer;
import java.util.TimerTask;

/* JADX INFO: loaded from: classes.dex */
public class MyTimeTask {
    private TimerTask task;
    private long time;
    private Timer timer;

    public MyTimeTask(long j, TimerTask timerTask) {
        this.task = timerTask;
        this.time = j;
        if (this.timer == null) {
            this.timer = new Timer();
        }
    }

    public void start() {
        this.timer.schedule(this.task, 0L, this.time);
    }

    public void stop() {
        Timer timer = this.timer;
        if (timer != null) {
            timer.cancel();
            TimerTask timerTask = this.task;
            if (timerTask != null) {
                timerTask.cancel();
            }
        }
    }
}