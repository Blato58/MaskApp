package cn.com.heaton.shiningmask.ui.dialog;

import android.content.Context;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseDialog;

/* JADX INFO: loaded from: classes.dex */
public class SyncImageDialog extends BaseDialog implements View.OnClickListener {
    private ImageView ivClose;
    private Context mContext;
    private ResultListener resultListener;
    private TextView tvSync;

    public interface ResultListener {
        void close();

        void sync();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected void onInitData() {
    }

    public SyncImageDialog(Context context) {
        super(context);
    }

    public SyncImageDialog(Context context, int i) {
        super(context, i);
        this.mContext = context;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected int getLayoutResource() {
        return R.layout.dialog_image_sync;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected void onInitView() {
        this.ivClose = (ImageView) findViewById(R.id.iv_close);
        this.tvSync = (TextView) findViewById(R.id.tv_sync);
        this.ivClose.setOnClickListener(this);
        this.tvSync.setOnClickListener(this);
        findViewById(R.id.ll_root).setOnClickListener(this);
    }

    public ResultListener getResultListener() {
        return this.resultListener;
    }

    public void setResultListener(ResultListener resultListener) {
        this.resultListener = resultListener;
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        ResultListener resultListener;
        int id = view.getId();
        if (id == R.id.ll_root) {
            dismiss();
            return;
        }
        if (id == R.id.iv_close) {
            ResultListener resultListener2 = this.resultListener;
            if (resultListener2 != null) {
                resultListener2.close();
                return;
            }
            return;
        }
        if (id != R.id.tv_sync || (resultListener = this.resultListener) == null) {
            return;
        }
        resultListener.sync();
    }
}