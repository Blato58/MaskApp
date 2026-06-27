package cn.com.heaton.shiningmask.ui.dialog;

import android.content.Context;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseDialog;

/* JADX INFO: loaded from: classes.dex */
public class ClearImageDialog extends BaseDialog implements View.OnClickListener {
    private ImageView ivClose;
    private Context mContext;
    private ResultListener resultListener;
    private TextView tvDevice;
    private TextView tvPhone;
    private TextView tvPhoneAndDevice;

    public interface ResultListener {
        void close();

        void device();

        void phone();

        void phoneAndDevice();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected void onInitData() {
    }

    public ClearImageDialog(Context context) {
        super(context);
    }

    public ClearImageDialog(Context context, int i) {
        super(context, i);
        this.mContext = context;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected int getLayoutResource() {
        return R.layout.dialog_image_clear;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseDialog
    protected void onInitView() {
        this.ivClose = (ImageView) findViewById(R.id.iv_close);
        this.tvPhone = (TextView) findViewById(R.id.tv_phone);
        this.tvDevice = (TextView) findViewById(R.id.tv_device);
        this.tvPhoneAndDevice = (TextView) findViewById(R.id.tv_phone_and_device);
        this.ivClose.setOnClickListener(this);
        this.tvPhone.setOnClickListener(this);
        this.tvDevice.setOnClickListener(this);
        this.tvPhoneAndDevice.setOnClickListener(this);
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
        if (id == R.id.tv_phone) {
            ResultListener resultListener3 = this.resultListener;
            if (resultListener3 != null) {
                resultListener3.phone();
                return;
            }
            return;
        }
        if (id == R.id.tv_device) {
            ResultListener resultListener4 = this.resultListener;
            if (resultListener4 != null) {
                resultListener4.device();
                return;
            }
            return;
        }
        if (id != R.id.tv_phone_and_device || (resultListener = this.resultListener) == null) {
            return;
        }
        resultListener.phoneAndDevice();
    }
}