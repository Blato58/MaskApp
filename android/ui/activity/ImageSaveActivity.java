package cn.com.heaton.shiningmask.ui.activity;

import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.view.LayoutInflater;
import android.view.View;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.app.MyTimeTask;
import cn.com.heaton.shiningmask.databinding.ActivityImageSaveBinding;
import cn.com.heaton.shiningmask.model.data.AnimData;
import cn.com.heaton.shiningmask.ui.fragment.AnimFragment;
import cn.com.heaton.shiningmask.ui.fragment.ImageFragment;
import cn.com.heaton.shiningmask.ui.utils.BitmapUtils;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import java.util.ArrayList;
import java.util.List;
import java.util.TimerTask;

/* JADX INFO: loaded from: classes.dex */
public class ImageSaveActivity extends BaseActivity<ActivityImageSaveBinding> implements View.OnClickListener {
    private FragmentManager fm;
    int index;
    int[][] list;
    private MyTimeTask task;
    private FragmentTransaction transaction;
    private int curSelect = 0;
    List<int[]> list1 = new ArrayList();
    int index22 = 22;
    final Handler handler = new Handler(Looper.getMainLooper()) { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSaveActivity.3
        @Override // android.os.Handler
        public void handleMessage(Message message) throws Throwable {
            if (message.what == 1001) {
                if (ImageSaveActivity.this.index >= ImageSaveActivity.this.list1.size()) {
                    return;
                }
                ((ActivityImageSaveBinding) ImageSaveActivity.this.getBinding()).ledviewSave.setImageData(ImageSaveActivity.this.list1.get(ImageSaveActivity.this.index));
                try {
                    Thread.sleep(50L);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                LogUtil.d("保存后的图片路径：" + BitmapUtils.saveToLocalPNG(BitmapUtils.captureView(((ActivityImageSaveBinding) ImageSaveActivity.this.getBinding()).ledviewSave), ImageSaveActivity.this.index, ImageSaveActivity.this.index22));
                ImageSaveActivity.this.index++;
            }
            super.handleMessage(message);
        }
    };

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityImageSaveBinding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityImageSaveBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().ivGallery.setOnClickListener(this);
        getBinding().ivAnim.setOnClickListener(this);
        getBinding().ivAnim.setImageResource(R.mipmap.image_anim_selected);
        getBinding().ivGallery.setImageResource(R.mipmap.image_gallery);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        initTab();
        getBinding().ledviewSave.setPointMargin(0);
        getBinding().ledviewSave.removeAllViews();
        getBinding().ledviewSave.init(36, 12);
        getBinding().ledviewSave.setLayerType(1, null);
        this.list1.add(AnimData.getAnim_22_Image0());
        this.list1.add(AnimData.getAnim_22_Image1());
        this.list1.add(AnimData.getAnim_22_Image2());
        this.list1.add(AnimData.getAnim_22_Image3());
        this.list1.add(AnimData.getAnim_22_Image4());
        this.list1.add(AnimData.getAnim_22_Image5());
        this.list1.add(AnimData.getAnim_22_Image6());
        this.list1.add(AnimData.getAnim_22_Image7());
        this.list1.add(AnimData.getAnim_22_Image8());
        this.list1.add(AnimData.getAnim_22_Image9());
        new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSaveActivity.1
            @Override // java.lang.Runnable
            public void run() {
                ImageSaveActivity.this.setTimer(true);
            }
        }, 2000L);
    }

    private void initTab() {
        FragmentManager supportFragmentManager = getSupportFragmentManager();
        this.fm = supportFragmentManager;
        FragmentTransaction fragmentTransactionBeginTransaction = supportFragmentManager.beginTransaction();
        this.transaction = fragmentTransactionBeginTransaction;
        fragmentTransactionBeginTransaction.replace(R.id.fl_change, AnimFragment.newInstance(false));
        this.transaction.commit();
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        FragmentManager supportFragmentManager = getSupportFragmentManager();
        this.fm = supportFragmentManager;
        this.transaction = supportFragmentManager.beginTransaction();
        int id = view.getId();
        if (id == R.id.iv_anim) {
            if (this.curSelect == 0) {
                return;
            }
            this.curSelect = 0;
            getBinding().ivAnim.setImageResource(R.mipmap.image_anim_selected);
            getBinding().ivGallery.setImageResource(R.mipmap.image_gallery);
            this.transaction.replace(R.id.fl_change, AnimFragment.newInstance(true));
        } else if (id == R.id.iv_gallery) {
            if (this.curSelect == 1) {
                return;
            }
            this.curSelect = 1;
            getBinding().ivGallery.setImageResource(R.mipmap.image_gallery_selected);
            getBinding().ivAnim.setImageResource(R.mipmap.image_anim);
            this.transaction.replace(R.id.fl_change, ImageFragment.newInstance(0));
        }
        this.transaction.commit();
    }

    public void setTimer(boolean z) {
        MyTimeTask myTimeTask = new MyTimeTask(500L, new TimerTask() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSaveActivity.2
            @Override // java.util.TimerTask, java.lang.Runnable
            public void run() {
                ImageSaveActivity.this.handler.sendEmptyMessage(1001);
            }
        });
        this.task = myTimeTask;
        myTimeTask.start();
    }
}