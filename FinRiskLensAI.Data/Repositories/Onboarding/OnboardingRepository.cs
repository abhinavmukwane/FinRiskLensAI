using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.IRepositories.AccountAggregator;
using FinRiskLensAI.Core.Interfaces.IRepositories.OnBoarding;
using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.Universal;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Data.Repositories.Onboarding
{
    public class OnboardingRepository : IOnboardingRepository
    {
        private readonly DbContextEDMX.ApplicationDbContext _context;
        public OnboardingRepository(DbContextEDMX.ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ResultModel<StaticResponseModel>> FetchUdyam(string uan)
        {
            var result = new ResultModel<StaticResponseModel>();

            try
            {
                var entity = await _context.StaticResponseModel.FirstOrDefaultAsync(x => x.UdyamNumber == uan);

                if (entity != null)
                {

                    var model = new StaticResponseModel();

                    entity.MapToModelObject(model);

                    result.Result = tflResultType.tflSuccess;
                    result.Message = "Data Fetched Successfully";
                    result.Data = model;
                }
                else
                {
                    result.Result = tflResultType.tflNoRecordFound;
                    result.Message = "No Data Found";
                    result.Data = null;
                }
            }
            catch (Exception ex)
            {
                result.Result = tflResultType.tflError;
                result.Message = ex.Message;
                result.Data = null;
            }

            return result;
        }

        //public async Task<MsmeEnquiry> SaveMsmeData(string json)
        //{
        //    var model = JsonConvert.DeserializeObject<UdyamResponseModel>(json);
        //    if (model == null)
        //        throw new ArgumentException("Invalid or empty JSON payload.", nameof(json));

        //    if (string.IsNullOrWhiteSpace(model.uan))
        //        throw new ArgumentException("UAN is required to save or update MSME data.");

        //    var excluded = new List<string> { "MsmeEnquiryID", "CreatedAt", "Uan" };

        //    await using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var enquiry = await _context.MsmeEnquiries
        //            .Include(x => x.Locations)
        //            .Include(x => x.NicCodes)
        //            .FirstOrDefaultAsync(x => x.Uan == model.uan);

        //        bool isNew = enquiry == null;

        //        if (isNew)
        //        {
        //            enquiry = new MsmeEnquiry
        //            {
        //                Uan = model.uan
        //            };
        //            _context.MsmeEnquiries.Add(enquiry);
        //        }
        //        else
        //        {
        //            _context.MsmeLocations.RemoveRange(enquiry.Locations);
        //            _context.MsmeNicCodes.RemoveRange(enquiry.NicCodes);
        //        }

        //        model.MapToModelObject(enquiry, excluded);
        //        model.main_details?.MapToModelObject(enquiry, excluded);
        //        enquiry.Payload = JsonConvert.SerializeObject(model);

        //        await _context.SaveChangesAsync();

        //        if (model.location_of_plant_details != null)
        //        {
        //            foreach (var item in model.location_of_plant_details)
        //            {
        //                var location = new MsmeLocation
        //                {
        //                    MsmeEnquiryID = enquiry.MsmeEnquiryID
        //                };
        //                item.MapToModelObject(location, new List<string> { "MsmeEnquiryID", "CreatedAt" });
        //                _context.MsmeLocations.Add(location);
        //            }
        //        }

        //        if (model.nic_code != null)
        //        {
        //            foreach (var item in model.nic_code)
        //            {
        //                var nic = new MsmeNicCode
        //                {
        //                    MsmeEnquiryID = enquiry.MsmeEnquiryID
        //                };
        //                item.MapToModelObject(nic, new List<string> { "MsmeEnquiryID", "CreatedAt" });
        //                _context.MsmeNicCodes.Add(nic);
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return enquiry;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

        public async Task SaveMsmeData(string json)
        {
            var model = JsonConvert.DeserializeObject<UdyamResponseModel>(json);

            if (model == null)
                return;

            bool exists = await _context.MsmeEnquiries.AnyAsync(x => x.Uan == model.uan);


            var enquiry = new MsmeEnquiry
            {
                ClientId = model.client_id,
                Uan = model.uan,
                CertificateUrl = model.certificate_url,
                NameOfEnterprise = model.main_details.name_of_enterprise,
                MajorActivity = model.main_details.major_activity,
                SocialCategory = model.main_details.social_category,
                DateOfCommencement = model.main_details.date_of_commencement,
                DicName = model.main_details.dic_name,
                State = model.main_details.state,
                AppliedDate = model.main_details.applied_date,

                Flat = model.main_details.flat,
                NameOfBuilding = model.main_details.name_of_building,
                Road = model.main_details.road,
                Village = model.main_details.village,
                Block = model.main_details.block,
                City = model.main_details.city,
                Pin = model.main_details.pin,

                MobileNumber = model.main_details.mobile_number,
                Email = model.main_details.email,
                OrganizationType = model.main_details.organization_type,
                Gender = model.main_details.gender,
                DateOfIncorporation = model.main_details.date_of_incorporation,
                MsmeDfo = model.main_details.msme_dfo,
                RegistrationDate = model.main_details.registration_date,

                Payload = JsonConvert.SerializeObject(model),

                CreatedAt = DateTime.Now
            };

            _context.MsmeEnquiries.Add(enquiry);

            await _context.SaveChangesAsync();

            if (model.location_of_plant_details != null)
            {
                foreach (var item in model.location_of_plant_details)
                {
                    var location = new MsmeLocation
                    {
                        MsmeEnquiryID = enquiry.MsmeEnquiryID,

                        UnitName = item.unit_name,
                        Line1 = item.line_1,
                        Building = item.building,
                        Village = item.village,
                        Street = item.street,
                        Road = item.road,
                        City = item.city,
                        Pin = item.pin,
                        State = item.state,
                        District = item.district,

                        CreatedAt = DateTime.Now
                    };

                    _context.MsmeLocations.Add(location);
                }
            }
            if (model.nic_code != null)
            {
                foreach (var item in model.nic_code)
                {
                    var nic = new MsmeNicCode
                    {
                        MsmeEnquiryID = enquiry.MsmeEnquiryID,

                        Nic2Digit = item.nic_2_digit,
                        Nic4Digit = item.nic_4_digit,
                        Nic5Digit = item.nic_5_digit,
                        ActivityType = item.activity_type,
                        AddedOn = item.added_on,

                        CreatedAt = DateTime.Now
                    };

                    _context.MsmeNicCodes.Add(nic);
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<UdyamDetailsModel> GetUdyamDetails(string uan)
        {
            var data = await (
                from e in _context.MsmeEnquiries
                join l in _context.MsmeLocations
                    on e.MsmeEnquiryID equals l.MsmeEnquiryID into loc
                from l in loc.DefaultIfEmpty()

                where e.Uan == uan

                select new
                {
                    e.Uan,e.NameOfEnterprise,e.OrganizationType, e.DateOfIncorporation,e.Email,
                    e.MobileNumber,l.Line1,l.Building,l.Road,l.Village,l.City, l.District, l.State,l.Pin
                }).FirstOrDefaultAsync();

            if (data == null)
                return null;

            return new UdyamDetailsModel
            {
                UdyamNumber = data.Uan,
                EnterpriseName = data.NameOfEnterprise,
                OrganizationType = data.OrganizationType,
                EnterpriseType = data.OrganizationType,
                DateOfIncorporation = data.DateOfIncorporation,
                Email = data.Email,
                Mobile = data.MobileNumber,

                Address = string.Join(", ", new[]
                {data.Line1, data.Building, data.Road,data.Village, data.City, data.District, data.State,
                    data.Pin}.Where(x => !string.IsNullOrWhiteSpace(x))),
                District = data.District,
                State = data.State,
                PinCode = data.Pin
            };
        }
        public async Task<ResultModel<UserRegistrationModel>> AddUpdateUserRegst(UserRegistrationModel entity)
        {
            var result = new ResultModel<UserRegistrationModel>();
            try
            {
                UserRegistrationModel paObj = new UserRegistrationModel();
                entity.MapToModelObject(paObj);
                _context.UserRegistration.Add(paObj);
                await _context.SaveChangesAsync();

                result.Result = tflResultType.tflSuccess;
                result.Message = "Data Saved Successfully";
                result.Data = entity;
            }
            catch (Exception ex)
            {
                result.Result = tflResultType.tflError;
                result.Message = ex.Message;
                result.Data = null;
            }
            return result;
        }

        //public async Task<ResultModel<RegisterModel>> AddUpdateOtp(RegisterModel entity)
        //{
        //    var result = new ResultModel<RegisterModel>();
        //    try
        //    {
        //        T_APPROVEDLOAN paObj = new T_APPROVEDLOAN();
        //        entity.MapToModelObject(paObj);
        //        _context.T_APPROVEDLOAN.Add(paObj);
        //        await _context.SaveChangesAsync();

        //        result.Result = tflResultType.tflSuccess;
        //        result.Message = "Data Saved Successfully";
        //        result.Data = entity;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Result = tflResultType.tflError;
        //        result.Message = ex.Message;
        //        result.Data = null;
        //    }
        //    return result;
        //}
    }
}
