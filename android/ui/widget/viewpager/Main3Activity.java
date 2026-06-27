package cn.com.heaton.shiningmask.ui.widget.viewpager;

import android.os.Bundle;
import android.view.View;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import androidx.appcompat.app.AppCompatActivity;
import androidx.viewpager.widget.ViewPager;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.model.bean.RhythmImage;
import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class Main3Activity extends AppCompatActivity {
    List<RhythmImage> list = new ArrayList();
    private LoopViewPager mViewpager;

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, androidx.core.app.ComponentActivity, android.app.Activity
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.activity_main3);
        initViewPager();
    }

    private void initViewPager() {
        this.mViewpager = (LoopViewPager) findViewById(R.id.myviewpager);
        ImageView imageView = new ImageView(this);
        ImageView imageView2 = new ImageView(this);
        ImageView imageView3 = new ImageView(this);
        ImageView imageView4 = new ImageView(this);
        RhythmImage rhythmImage = new RhythmImage(R.mipmap.rhy_bg01, true, imageView);
        RhythmImage rhythmImage2 = new RhythmImage(R.mipmap.rhy_bg02, true, imageView2);
        RhythmImage rhythmImage3 = new RhythmImage(R.mipmap.rhy_bg03, true, imageView3);
        RhythmImage rhythmImage4 = new RhythmImage(R.mipmap.rhy_bg04, true, imageView4);
        this.list.add(rhythmImage);
        this.list.add(rhythmImage2);
        this.list.add(rhythmImage3);
        this.list.add(rhythmImage4);
        this.mViewpager.setClipChildren(false);
        this.mViewpager.setPageMargin((-getResources().getDisplayMetrics().widthPixels) / 13);
        RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams) this.mViewpager.getLayoutParams();
        layoutParams.height = (int) (((double) (getResources().getDisplayMetrics().widthPixels * 330)) / 750.0d);
        layoutParams.width = getResources().getDisplayMetrics().widthPixels;
        this.mViewpager.setLayoutParams(layoutParams);
        this.mViewpager.setAdapter(new MyAdapter(this, this.list));
        this.mViewpager.setPageTransformer(true, new ViewPager.PageTransformer() { // from class: cn.com.heaton.shiningmask.ui.widget.viewpager.Main3Activity.1
            float scale = 0.85f;

            @Override // androidx.viewpager.widget.ViewPager.PageTransformer
            public void transformPage(View view, float f) {
                if (f >= 0.0f && f <= 1.0f) {
                    float f2 = this.scale;
                    view.setScaleY(f2 + ((1.0f - f2) * (1.0f - f)));
                } else if (f > -1.0f && f < 0.0f) {
                    view.setScaleY(((1.0f - this.scale) * f) + 1.0f);
                } else {
                    view.setScaleY(this.scale);
                }
            }
        });
        this.mViewpager.autoLoop(true);
    }
}