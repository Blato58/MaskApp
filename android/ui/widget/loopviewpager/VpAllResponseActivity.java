package cn.com.heaton.shiningmask.ui.widget.loopviewpager;

import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.RelativeLayout;
import androidx.appcompat.app.AppCompatActivity;
import androidx.viewpager.widget.ViewPager;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.model.bean.RhythmImage;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public class VpAllResponseActivity extends AppCompatActivity implements ViewPager.OnPageChangeListener {
    private int currentIndex;
    ArrayList<RhythmImage> list = new ArrayList<>();
    private RelativeLayout mRoot;
    private ViewPagerAllResponse mVp;

    @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
    public void onPageScrolled(int i, float f, int i2) {
    }

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, androidx.core.app.ComponentActivity, android.app.Activity
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.activity_vp_all_response);
        initView();
        initClick();
        initData();
    }

    private void initView() {
        this.mRoot = (RelativeLayout) findViewById(R.id.root);
        this.mVp = (ViewPagerAllResponse) findViewById(R.id.vp);
    }

    private void initClick() {
        this.mRoot.setOnTouchListener(new View.OnTouchListener() { // from class: cn.com.heaton.shiningmask.ui.widget.loopviewpager.VpAllResponseActivity.1
            @Override // android.view.View.OnTouchListener
            public boolean onTouch(View view, MotionEvent motionEvent) {
                return VpAllResponseActivity.this.mVp.dispatchTouchEvent(motionEvent);
            }
        });
    }

    private void initData() {
        RhythmImage rhythmImage = new RhythmImage(R.mipmap.rhyhm_mode_bg1, true);
        RhythmImage rhythmImage2 = new RhythmImage(R.mipmap.rhyhm_mode_bg2, true);
        RhythmImage rhythmImage3 = new RhythmImage(R.mipmap.rhyhm_mode_bg3, true);
        RhythmImage rhythmImage4 = new RhythmImage(R.mipmap.rhyhm_mode_bg4, true);
        RhythmImage rhythmImage5 = new RhythmImage(R.mipmap.rhyhm_mode_bg5, true);
        this.list.add(rhythmImage);
        this.list.add(rhythmImage2);
        this.list.add(rhythmImage3);
        this.list.add(rhythmImage4);
        this.list.add(rhythmImage5);
        RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams) this.mRoot.getLayoutParams();
        layoutParams.width = ScreenUtils.getScreenWidth(this);
        this.mRoot.setLayoutParams(layoutParams);
        RelativeLayout.LayoutParams layoutParams2 = (RelativeLayout.LayoutParams) this.mVp.getLayoutParams();
        layoutParams2.width = (int) (((double) ScreenUtils.getScreenWidth(this)) / 1.3d);
        layoutParams2.height = (int) (((double) ScreenUtils.getScreenWidth(this)) / 1.3d);
        this.mVp.setLayoutParams(layoutParams2);
        this.mVp.setAdapter(new ViewpagerAdapter(this, this.mVp, this.list));
        this.mVp.setPageTransformer(true, new ZoomOutPageTransformer());
        this.mVp.setPageMargin((int) ScreenUtils.dpToPx(this, -60.0f));
        this.mVp.setCurrentItem(5000);
        this.mVp.setOffscreenPageLimit(2);
        this.mVp.addOnPageChangeListener(this);
    }

    @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
    public void onPageScrollStateChanged(int i) {
        Log.d("ViewPage", " state:" + i);
        if (i == 2) {
            Log.d("TAG", "开始滑动");
        } else if (i == 0) {
            Log.d("TAG", "停止");
        }
    }

    @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
    public void onPageSelected(int i) {
        this.currentIndex = i % 5;
        Log.d("ViewPage", i + " currentIndex:" + this.currentIndex);
    }
}