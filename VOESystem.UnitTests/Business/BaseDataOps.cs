using System.Collections.Generic;
using ServiceStack.OrmLite;

namespace VOESystem.UnitTests.Business
{
    public class BaseDataOps : BusinessBase
    {

        public Data.DTO.VendorListResp getVendorList()
        {

            Data.DTO.VendorListResp retVal = null;


            Data.Business.BaseDataOps bOp = new Data.Business.BaseDataOps();
            retVal = bOp.getVendorList(Db);
                

            return retVal;

        }


        public List<Data.DBSchema.UserRoleView> getVOESpecialistList()
        {

            Data.DTO.VOESpecialistListResp retVal = null;

            Data.Business.BaseDataOps bOp = new Data.Business.BaseDataOps();
            retVal = bOp.getVOESpecialistList(Db, UserName, true);

            return retVal.VOESpecialistList;

        }

        public List<VOESystem.Data.DBSchema.OrderStatus> getOrderStatuses(bool ParentsOnly)
        {

            Data.Business.BaseDataOps bOp = new Data.Business.BaseDataOps();
            return bOp.getOrderStatusList(Db, ParentsOnly);

        }

        public List<VOESystem.Data.DBSchema.RequestType> getRequestTypes()
        {

            Data.Business.BaseDataOps bOp = new Data.Business.BaseDataOps();
            return bOp.getRequestTypeList(Db).RequestTypeList;

        }
    }
}
