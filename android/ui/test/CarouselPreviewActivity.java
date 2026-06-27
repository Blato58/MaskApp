package cn.com.heaton.shiningmask.ui.test;

import android.app.Activity;
import android.os.Bundle;
import android.os.Handler;
import androidx.recyclerview.widget.RecyclerView;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.adapter.TextImageIconAdapter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselLayoutManager;
import cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselZoomPostLayoutListener;
import cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CenterScrollListener;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public class CarouselPreviewActivity extends Activity {
    private Activity mActivity;
    private RecyclerView recyclerView;
    private TextImageIconAdapter textImageIconAdapter;

    @Override // android.app.Activity
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.activity_carousel_preview);
        this.mActivity = this;
        RecyclerView recyclerView = (RecyclerView) findViewById(R.id.list_horizontal_menu);
        this.recyclerView = recyclerView;
        initRecyclerView(recyclerView, new CarouselLayoutManager(0, true));
    }

    public void initRecyclerView(RecyclerView recyclerView, CarouselLayoutManager carouselLayoutManager) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Integer.valueOf(R.mipmap.main_menu_image));
        arrayList.add(Integer.valueOf(R.mipmap.main_menu_text));
        arrayList.add(Integer.valueOf(R.mipmap.main_menu_music));
        arrayList.add(Integer.valueOf(R.mipmap.main_menu_diy));
        this.textImageIconAdapter = new TextImageIconAdapter(this, R.layout.item_image, arrayList);
        carouselLayoutManager.setPostLayoutListener(new CarouselZoomPostLayoutListener());
        carouselLayoutManager.setMaxVisibleItems(1);
        recyclerView.setLayoutManager(carouselLayoutManager);
        recyclerView.setHasFixedSize(true);
        recyclerView.setAdapter(this.textImageIconAdapter);
        this.textImageIconAdapter.setOniClickListener(new TextImageIconAdapter.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.test.CarouselPreviewActivity.1
            @Override // cn.com.heaton.shiningmask.ui.adapter.TextImageIconAdapter.OnItemClickListener
            public void onClick(int i) {
                LogUtil.d("index:" + i);
            }
        });
        recyclerView.addOnScrollListener(new CenterScrollListener());
        carouselLayoutManager.addOnItemSelectionListener(new CarouselLayoutManager.OnCenterItemSelectionListener() { // from class: cn.com.heaton.shiningmask.ui.test.CarouselPreviewActivity.2
            @Override // cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselLayoutManager.OnCenterItemSelectionListener
            public void onCenterItemChanged(int i) {
                LogUtil.d("adapterPosition:" + i);
                new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.test.CarouselPreviewActivity.2.1
                    @Override // java.lang.Runnable
                    public void run() {
                    }
                }, 50L);
            }
        });
    }
}