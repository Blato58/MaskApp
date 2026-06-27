package cn.com.heaton.shiningmask.ui.widget.loopviewpager;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import androidx.viewpager.widget.PagerAdapter;
import androidx.viewpager.widget.ViewPager;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.model.bean.RhythmImage;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class ViewpagerAdapter extends PagerAdapter {
    private static final String TAG = "ViewpagerAdapter";
    private Context mContext;
    private LayoutInflater mInflater;
    private List<RhythmImage> mList;
    private final int mMaxNumber;
    private ViewPager mVp;

    @Override // androidx.viewpager.widget.PagerAdapter
    public int getCount() {
        return Integer.MAX_VALUE;
    }

    @Override // androidx.viewpager.widget.PagerAdapter
    public boolean isViewFromObject(View view, Object obj) {
        return view == obj;
    }

    public ViewpagerAdapter(Context context, ViewPager viewPager, List<RhythmImage> list) {
        this.mContext = context;
        this.mVp = viewPager;
        this.mList = list;
        this.mInflater = LayoutInflater.from(context);
        if (list.size() > 9) {
            this.mMaxNumber = 9;
        } else {
            this.mMaxNumber = list.size();
        }
    }

    @Override // androidx.viewpager.widget.PagerAdapter
    public void destroyItem(ViewGroup viewGroup, int i, Object obj) {
        viewGroup.removeView((View) obj);
    }

    @Override // androidx.viewpager.widget.PagerAdapter
    public Object instantiateItem(ViewGroup viewGroup, int i) {
        RhythmImage rhythmImage = this.mList.get(i % this.mMaxNumber);
        View viewInflate = this.mInflater.inflate(R.layout.item_rhy_image, (ViewGroup) null);
        ((ImageView) viewInflate.findViewById(R.id.iv_rhy_image)).setImageResource(rhythmImage.getImageRes());
        viewInflate.setTag(Integer.valueOf(i));
        viewGroup.addView(viewInflate);
        return viewInflate;
    }
}