package cn.com.heaton.shiningmask.dao;

import cn.com.heaton.shiningmask.dao.bean.CropImage;
import cn.com.heaton.shiningmask.dao.bean.Device;
import cn.com.heaton.shiningmask.dao.bean.InputTextRecord;
import cn.com.heaton.shiningmask.model.bean.DiyData;
import cn.com.heaton.shiningmask.model.bean.HistoryData;
import java.util.Map;
import org.greenrobot.greendao.AbstractDao;
import org.greenrobot.greendao.AbstractDaoSession;
import org.greenrobot.greendao.database.Database;
import org.greenrobot.greendao.identityscope.IdentityScopeType;
import org.greenrobot.greendao.internal.DaoConfig;

/* JADX INFO: loaded from: classes.dex */
public class DaoSession extends AbstractDaoSession {
    private final CropImageDao cropImageDao;
    private final DaoConfig cropImageDaoConfig;
    private final DeviceDao deviceDao;
    private final DaoConfig deviceDaoConfig;
    private final DiyDataDao diyDataDao;
    private final DaoConfig diyDataDaoConfig;
    private final HistoryDataDao historyDataDao;
    private final DaoConfig historyDataDaoConfig;
    private final InputTextRecordDao inputTextRecordDao;
    private final DaoConfig inputTextRecordDaoConfig;

    public DaoSession(Database database, IdentityScopeType identityScopeType, Map<Class<? extends AbstractDao<?, ?>>, DaoConfig> map) {
        super(database);
        DaoConfig daoConfigM2265clone = map.get(CropImageDao.class).clone();
        this.cropImageDaoConfig = daoConfigM2265clone;
        daoConfigM2265clone.initIdentityScope(identityScopeType);
        DaoConfig daoConfigM2265clone2 = map.get(DeviceDao.class).clone();
        this.deviceDaoConfig = daoConfigM2265clone2;
        daoConfigM2265clone2.initIdentityScope(identityScopeType);
        DaoConfig daoConfigM2265clone3 = map.get(InputTextRecordDao.class).clone();
        this.inputTextRecordDaoConfig = daoConfigM2265clone3;
        daoConfigM2265clone3.initIdentityScope(identityScopeType);
        DaoConfig daoConfigM2265clone4 = map.get(DiyDataDao.class).clone();
        this.diyDataDaoConfig = daoConfigM2265clone4;
        daoConfigM2265clone4.initIdentityScope(identityScopeType);
        DaoConfig daoConfigM2265clone5 = map.get(HistoryDataDao.class).clone();
        this.historyDataDaoConfig = daoConfigM2265clone5;
        daoConfigM2265clone5.initIdentityScope(identityScopeType);
        CropImageDao cropImageDao = new CropImageDao(daoConfigM2265clone, this);
        this.cropImageDao = cropImageDao;
        DeviceDao deviceDao = new DeviceDao(daoConfigM2265clone2, this);
        this.deviceDao = deviceDao;
        InputTextRecordDao inputTextRecordDao = new InputTextRecordDao(daoConfigM2265clone3, this);
        this.inputTextRecordDao = inputTextRecordDao;
        DiyDataDao diyDataDao = new DiyDataDao(daoConfigM2265clone4, this);
        this.diyDataDao = diyDataDao;
        HistoryDataDao historyDataDao = new HistoryDataDao(daoConfigM2265clone5, this);
        this.historyDataDao = historyDataDao;
        registerDao(CropImage.class, cropImageDao);
        registerDao(Device.class, deviceDao);
        registerDao(InputTextRecord.class, inputTextRecordDao);
        registerDao(DiyData.class, diyDataDao);
        registerDao(HistoryData.class, historyDataDao);
    }

    public void clear() {
        this.cropImageDaoConfig.clearIdentityScope();
        this.deviceDaoConfig.clearIdentityScope();
        this.inputTextRecordDaoConfig.clearIdentityScope();
        this.diyDataDaoConfig.clearIdentityScope();
        this.historyDataDaoConfig.clearIdentityScope();
    }

    public CropImageDao getCropImageDao() {
        return this.cropImageDao;
    }

    public DeviceDao getDeviceDao() {
        return this.deviceDao;
    }

    public InputTextRecordDao getInputTextRecordDao() {
        return this.inputTextRecordDao;
    }

    public DiyDataDao getDiyDataDao() {
        return this.diyDataDao;
    }

    public HistoryDataDao getHistoryDataDao() {
        return this.historyDataDao;
    }
}