package cn.com.heaton.shiningmask.ui.dialog;

import android.content.Context;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseDialog;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.ScreenUtils;

/* JADX INFO: loaded from: classes.dex */
public class SyncProgressDialog extends BaseDialog {
    private ImageView ivProgress;
    private LinearLayout llTop;
    private LinearLayout llTvProgress;
    private Context mContext;
    private ProgressBar pbSync;
    private ResultListener resultListener;
    private int rlSeekBarWidth;
    private TextView tvProgress;

    public interface ResultListener {
        void progressResult(int i);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected void onInitData() {
    }

    public SyncProgressDialog(Context context) {
        super(context);
    }

    public SyncProgressDialog(Context context, int i) {
        super(context, i);
        this.mContext = context;
        setCancelable(false);
        setCanceledOnTouchOutside(false);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected int getLayoutResource() {
        return R.layout.dialog_sync_progress;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected void onInitView() {
        this.tvProgress = (TextView) findViewById(R.id.tv_progress);
        this.ivProgress = (ImageView) findViewById(R.id.iv_progress);
        this.pbSync = (ProgressBar) findViewById(R.id.pb_sync);
        this.llTvProgress = (LinearLayout) findViewById(R.id.ll_tv_progress);
        this.llTop = (LinearLayout) findViewById(R.id.ll_top);
    }

    @Override // android.app.Dialog
    public void onBackPressed() {
        LogUtil.d("========onBackPressed");
    }

    public void setProgress(int i) {
        ProgressBar progressBar = this.pbSync;
        if (progressBar != null) {
            progressBar.setProgress(i);
            this.tvProgress.setText(i + "%");
            setTextPro(i);
        }
    }

    public ResultListener getResultListener() {
        return this.resultListener;
    }

    public void setResultListener(ResultListener resultListener) {
        this.resultListener = resultListener;
    }

    private void setTextPro(int i) {
        this.rlSeekBarWidth = this.pbSync.getWidth() - ScreenUtils.dp2px(this.mContext, 0.0f);
        LogUtil.d("tvProWidth：" + this.llTvProgress.getWidth() + " rlSeekBarWidth:" + this.rlSeekBarWidth);
        float f = (this.rlSeekBarWidth * (i / 100.0f)) - (r0 / 2);
        LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(-2, -2);
        layoutParams.leftMargin = (int) f;
        this.llTvProgress.setLayoutParams(layoutParams);
    }
}