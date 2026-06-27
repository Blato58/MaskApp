package cn.com.heaton.shiningmask.ui.dialog;

import android.content.Context;
import android.view.View;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseDialog;

/* JADX INFO: loaded from: classes.dex */
public class PicturesChooseDialog extends BaseDialog implements View.OnClickListener {
    private Context mContext;
    private ResultListener resultListener;

    public interface ResultListener {
        void camera();

        void importImage();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected void onInitData() {
    }

    public PicturesChooseDialog(Context context) {
        super(context);
    }

    public PicturesChooseDialog(Context context, int i) {
        super(context, i);
        this.mContext = context;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected int getLayoutResource() {
        return R.layout.dialog_pictures_choose;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected void onInitView() {
        findViewById(R.id.ll_root).setOnClickListener(this);
        findViewById(R.id.iv_import).setOnClickListener(this);
        findViewById(R.id.iv_camera).setOnClickListener(this);
    }

    public ResultListener getResultListener() {
        return this.resultListener;
    }

    public void setResultListener(ResultListener resultListener) {
        this.resultListener = resultListener;
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        int id = view.getId();
        if (id == R.id.ll_root) {
            dismiss();
            return;
        }
        if (id == R.id.iv_import) {
            if (this.resultListener != null) {
                dismiss();
                this.resultListener.importImage();
                return;
            }
            return;
        }
        if (id == R.id.iv_camera) {
            dismiss();
            ResultListener resultListener = this.resultListener;
            if (resultListener != null) {
                resultListener.camera();
            }
        }
    }
}