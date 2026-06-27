package cn.com.heaton.shiningmask.ui.roteview;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Toast;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.roteview.ArcDragMenu;

/* JADX INFO: loaded from: classes.dex */
public class ArcDragMenuActivity extends Activity {
    private ArcDragMenu arcDragMenu;
    private int[] mItemImgs = {R.drawable.home_tianjia, R.drawable.home_tianjia, R.drawable.home_tianjia, R.drawable.home_tianjia, R.drawable.home_tianjia, R.drawable.home_tianjia, R.drawable.home_tianjia, R.drawable.home_tianjia, R.drawable.home_tianjia, R.drawable.home_tianjia};

    @Override // android.app.Activity
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.activity_arcdragmenu);
        ArcDragMenu arcDragMenu = (ArcDragMenu) findViewById(R.id.arcdragmenu);
        this.arcDragMenu = arcDragMenu;
        arcDragMenu.setMenuItemIcons(this.mItemImgs);
        this.arcDragMenu.setOnMenuItemClickListener(new ArcDragMenu.OnMenuItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.roteview.ArcDragMenuActivity.1
            @Override // cn.com.heaton.shiningmask.ui.roteview.ArcDragMenu.OnMenuItemClickListener
            public void onItemClick(View view, int i) {
                Toast.makeText(ArcDragMenuActivity.this, i + "", 0).show();
            }
        });
    }
}