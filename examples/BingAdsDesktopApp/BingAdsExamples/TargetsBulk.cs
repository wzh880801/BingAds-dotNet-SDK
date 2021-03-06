﻿// Copyright 2014 Microsoft Corporation 

// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 

//    http://www.apache.org/licenses/LICENSE-2.0 

// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.BingAds;
using Microsoft.BingAds.Bulk;
using Microsoft.BingAds.Bulk.Entities;
using Microsoft.BingAds.CampaignManagement;

namespace BingAdsExamples
{
    /// <summary>
    /// This example demonstrates how to add and update targets using the BulkServiceManager class.
    /// </summary>
    public class TargetsBulk : BulkExampleBase
    {
        public override string Description
        {
            get { return "BulkServiceManager | Targets"; }
        }

        public async override Task RunAsync(AuthorizationData authorizationData)
        {
            try
            {
                BulkService = new BulkServiceManager(authorizationData);

                var progress = new Progress<BulkOperationProgressInfo>(x =>
                    OutputStatusMessage(String.Format("{0} % Complete",
                        x.PercentComplete.ToString(CultureInfo.InvariantCulture))));

                #region Add

                const int targetIdKey = -1;
                const int campaignIdKey = -123;

                // Prepare the bulk entities that you want to upload. Each bulk entity contains the corresponding campaign management object, 
                // and additional elements needed to read from and write to a bulk file. 

                var bulkCampaign = new BulkCampaign
                {
                    // ClientId may be used to associate records in the bulk upload file with records in the results file. The value of this field 
                    // is not used or stored by the server; it is simply copied from the uploaded record to the corresponding result record.
                    // Note: This bulk file Client Id is not related to an application Client Id for OAuth. 
                    ClientId = "YourClientIdGoesHere",
                    Campaign = new Campaign
                    {
                        // When using the Campaign Management service, the Id cannot be set. In the context of a BulkCampaign, the Id is optional 
                        // and may be used as a negative reference key during bulk upload. For example the same negative value set for the campaign Id 
                        // will be used when associating this new campaign with a new call ad extension in the BulkCampaignCallAdExtension object below. 
                        Id = campaignIdKey,
                        Name = "Women's Shoes " + DateTime.UtcNow,
                        Description = "Red shoes line.",
                        BudgetType = BudgetLimitType.MonthlyBudgetSpendUntilDepleted,
                        MonthlyBudget = 1000.00,
                        TimeZone = "PacificTimeUSCanadaTijuana",
                        DaylightSaving = true
                    }
                };

                // Prepare targets for upload

                var bulkCampaignDayTimeTarget = new BulkCampaignDayTimeTarget
                {
                    CampaignId = campaignIdKey,
                    TargetId = targetIdKey,
                    DayTimeTarget = new DayTimeTarget
                    {
                        Bids = new List<DayTimeTargetBid>
                        {
                            new DayTimeTargetBid
                            {
                                BidAdjustment = 10,
                                Day = Day.Friday,
                                FromHour = 11,
                                FromMinute = Minute.Zero,
                                ToHour = 13,
                                ToMinute = Minute.Fifteen
                            },
                            new DayTimeTargetBid
                            {
                                BidAdjustment = 20,
                                Day = Day.Saturday,
                                FromHour = 11,
                                FromMinute = Minute.Zero,
                                ToHour = 13,
                                ToMinute = Minute.Fifteen
                            }
                        }
                    }
                };

                var bulkCampaignLocationTarget = new BulkCampaignLocationTarget
                {
                    CampaignId = campaignIdKey,
                    TargetId = targetIdKey,

                    IntentOption = IntentOption.PeopleIn,
                    CityTarget = new CityTarget
                    {
                        Bids = new List<CityTargetBid>
                        {
                            new CityTargetBid
                            {
                                BidAdjustment = 15,
                                City = "Toronto, Toronto ON CA",
                                IsExcluded = false
                            }
                        }
                    },
                    CountryTarget = new CountryTarget
                    {
                        Bids = new List<CountryTargetBid>
                        {
                            new CountryTargetBid
                            {
                                BidAdjustment = 15,
                                CountryAndRegion = "CA",
                                IsExcluded = false
                            }
                        }
                    },
                    MetroAreaTarget = new MetroAreaTarget
                    {
                        Bids = new List<MetroAreaTargetBid>
                        {
                            new MetroAreaTargetBid
                            {
                                BidAdjustment = 15,
                                MetroArea = "Seattle-Tacoma, WA, WA US",
                                IsExcluded = false
                            }
                        }
                    },
                    StateTarget = new StateTarget
                    {
                        Bids = new List<StateTargetBid>
                        {
                            new StateTargetBid
                            {
                                BidAdjustment = 15,
                                State = "US-WA",
                                IsExcluded = false
                            }
                        }
                    },
                    PostalCodeTarget = new PostalCodeTarget
                    {
                        Bids = new List<PostalCodeTargetBid>
                        {
                            new PostalCodeTargetBid
                            {
                                // Bid adjustments are not allowed for location exclusions. 
                                // If IsExcluded is true, this element will be ignored.
                                BidAdjustment = 10,
                                PostalCode = "98052, WA US",
                                IsExcluded = false
                            }
                        }
                    }
                };

                var bulkCampaignRadiusTarget = new BulkCampaignRadiusTarget
                {
                    CampaignId = campaignIdKey,
                    TargetId = targetIdKey,

                    RadiusTarget = new RadiusTarget2
                    {
                        Bids = new List<RadiusTargetBid2>
                        {
                            new RadiusTargetBid2
                            {
                                BidAdjustment = 50,
                                LatitudeDegrees = 47.755367,
                                LongitudeDegrees = -122.091827,
                                Radius = 11,
                                RadiusUnit = DistanceUnit.Kilometers
                            }
                        }
                    }
                };


                // Write the entities created above, to the specified file.
                // Dependent entities such as BulkCampaignLocationTarget must be written after any dependencies,  
                // for example a BulkCampaign. 

                Writer = new BulkFileWriter(FileDirectory + UploadFileName);

                Writer.WriteEntity(bulkCampaign);

                Writer.WriteEntity(bulkCampaignDayTimeTarget);
                Writer.WriteEntity(bulkCampaignLocationTarget);
                Writer.WriteEntity(bulkCampaignRadiusTarget);

                Writer.Dispose();

                var fileUploadParameters = new FileUploadParameters
                {
                    ResultFileDirectory = FileDirectory,
                    ResultFileName = ResultFileName,
                    OverwriteResultFile = true,
                    UploadFilePath = FileDirectory + UploadFileName,
                    ResponseMode = ResponseMode.ErrorsAndResults
                };

                // UploadFileAsync will upload the file you finished writing and will download the results file

                OutputStatusMessage("Starting UploadFileAsync . . .\n");
                var bulkFilePath =
                    await BulkService.UploadFileAsync(fileUploadParameters, progress, CancellationToken.None);
                Reader = new BulkFileReader(bulkFilePath, ResultFileType.Upload, FileType);
                OutputStatusMessage("Upload Results Bulk File Path" + Reader.BulkFilePath + "\n");
                OutputStatusMessage("Added Entities\n");

                // Write the upload output

                var bulkEntities = Reader.ReadEntities().ToList();

                var campaignResults = bulkEntities.OfType<BulkCampaign>().ToList();
                OutputBulkCampaigns(campaignResults);

                var campaignDayTimeTargetResults = bulkEntities.OfType<BulkCampaignDayTimeTarget>().ToList();
                OutputBulkCampaignDayTimeTargets(campaignDayTimeTargetResults);

                var campaignLocationTargetResults = bulkEntities.OfType<BulkCampaignLocationTarget>().ToList();
                OutputBulkCampaignLocationTargets(campaignLocationTargetResults);

                var campaignRadiusTargetResults = bulkEntities.OfType<BulkCampaignRadiusTarget>().ToList();
                OutputBulkCampaignRadiusTargets(campaignRadiusTargetResults);

                Reader.Dispose();

                #endregion Add

                #region Update

                // Update the day and time target. 
                // Do not create a BulkAdGroupDayTimeTarget for update, unless you want to replace all existing DayTime target bids
                // with the specified day and time target set for the current bulk upload. 
                // Instead you should upload one or more bids as a list of BulkCampaignDayTimeTargetBid.

                var bulkCampaignDayTimeTargetBids = new List<BulkCampaignDayTimeTargetBid>
                {
                    new BulkCampaignDayTimeTargetBid
                    {
                        CampaignId = campaignDayTimeTargetResults[0].CampaignId,
                        TargetId = targetIdKey,
                        DayTimeTargetBid = new DayTimeTargetBid
                        {
                            BidAdjustment = 15,
                            Day = Day.Friday,
                            FromHour = 11,
                            FromMinute = Minute.Zero,
                            ToHour = 13,
                            ToMinute = Minute.Fifteen
                        }
                    }
                };

                // Write the updated target to the file

                Writer = new BulkFileWriter(FileDirectory + UploadFileName);

                foreach (var bulkCampaignDayTimeTargetBid in bulkCampaignDayTimeTargetBids)
                {
                    Writer.WriteEntity(bulkCampaignDayTimeTargetBid);
                }

                Writer.Dispose();

                fileUploadParameters = new FileUploadParameters
                {
                    ResultFileDirectory = FileDirectory,
                    ResultFileName = ResultFileName,
                    OverwriteResultFile = true,
                    UploadFilePath = FileDirectory + UploadFileName,
                    ResponseMode = ResponseMode.ErrorsAndResults
                };

                // UploadFileAsync will upload the file you finished writing and will download the results file

                OutputStatusMessage("Starting UploadFileAsync . . .\n");
                bulkFilePath = await BulkService.UploadFileAsync(fileUploadParameters, progress, CancellationToken.None);
                Reader = new BulkFileReader(bulkFilePath, ResultFileType.Upload, FileType);
                OutputStatusMessage("Upload Results Bulk File Path: " + Reader.BulkFilePath + "\n");
                OutputStatusMessage("Updated Entities\n");


                // Write any upload errors

                var campaignDayTimeTargetBidResults =
                    Reader.ReadEntities().OfType<BulkCampaignDayTimeTargetBid>().ToList();
                OutputBulkCampaignDayTimeTargetBids(campaignDayTimeTargetBidResults);

                Reader.Dispose();

                #endregion Update

                #region Delete

                // Prepare the bulk entities that you want to delete. You must set the Id field to the corresponding 
                // entity identifier, and the Status field to Deleted. 

                var campaignId = campaignResults[0].Campaign.Id;
                bulkCampaign = new BulkCampaign
                {
                    Campaign = new Campaign
                    {
                        Id = campaignId,
                        Status = CampaignStatus.Deleted
                    }
                };

                var targetId = campaignDayTimeTargetResults[0].TargetId;

                bulkCampaignDayTimeTarget = new BulkCampaignDayTimeTarget
                {
                    Status = Status.Deleted,
                    CampaignId = campaignId,
                    TargetId = targetId
                };

                bulkCampaignLocationTarget = new BulkCampaignLocationTarget
                {
                    Status = Status.Deleted,
                    CampaignId = campaignId,
                    TargetId = targetId
                };

                bulkCampaignRadiusTarget = new BulkCampaignRadiusTarget
                {
                    Status = Status.Deleted,
                    CampaignId = campaignId,
                    TargetId = targetId
                };
                

                // Write the entities that you want deleted, to the specified file.

                Writer = new BulkFileWriter(FileDirectory + UploadFileName);

                Writer.WriteEntity(bulkCampaign);

                Writer.WriteEntity(bulkCampaignDayTimeTarget);
                Writer.WriteEntity(bulkCampaignLocationTarget);
                Writer.WriteEntity(bulkCampaignRadiusTarget);

                Writer.Dispose();

                fileUploadParameters = new FileUploadParameters
                {
                    ResultFileDirectory = FileDirectory,
                    ResultFileName = ResultFileName,
                    OverwriteResultFile = true,
                    UploadFilePath = FileDirectory + UploadFileName,
                    ResponseMode = ResponseMode.ErrorsAndResults
                };

                // UploadFileAsync will upload the file you finished writing and will download the results file

                OutputStatusMessage("Starting UploadFileAsync . . .\n");
                bulkFilePath = await BulkService.UploadFileAsync(fileUploadParameters, progress, CancellationToken.None);
                Reader = new BulkFileReader(bulkFilePath, ResultFileType.Upload, FileType);
                OutputStatusMessage("Upload Results Bulk File Path" + Reader.BulkFilePath + "\n");
                OutputStatusMessage("Deleted Entities\n");

                // Write the upload output

                bulkEntities = Reader.ReadEntities().ToList();

                campaignResults = bulkEntities.OfType<BulkCampaign>().ToList();
                OutputBulkCampaigns(campaignResults);

                campaignDayTimeTargetResults = bulkEntities.OfType<BulkCampaignDayTimeTarget>().ToList();
                OutputBulkCampaignDayTimeTargets(campaignDayTimeTargetResults);

                campaignLocationTargetResults = bulkEntities.OfType<BulkCampaignLocationTarget>().ToList();
                OutputBulkCampaignLocationTargets(campaignLocationTargetResults);

                campaignRadiusTargetResults = bulkEntities.OfType<BulkCampaignRadiusTarget>().ToList();
                OutputBulkCampaignRadiusTargets(campaignRadiusTargetResults);

                Reader.Dispose();

                #endregion Delete
            }
            // Catch Microsoft Account authorization exceptions.
            catch (OAuthTokenRequestException ex)
            {
                OutputStatusMessage(string.Format("Couldn't get OAuth tokens. Error: {0}. Description: {1}", ex.Details.Error, ex.Details.Description));
            }
            // Catch Bulk service exceptions
            catch (FaultException<Microsoft.BingAds.Bulk.AdApiFaultDetail> ex)
            {
                OutputStatusMessage(string.Join("; ", ex.Detail.Errors.Select(error => string.Format("{0}: {1}", error.Code, error.Message))));
            }
            catch (FaultException<Microsoft.BingAds.Bulk.ApiFaultDetail> ex)
            {
                OutputStatusMessage(string.Join("; ", ex.Detail.OperationErrors.Select(error => string.Format("{0}: {1}", error.Code, error.Message))));
                OutputStatusMessage(string.Join("; ", ex.Detail.BatchErrors.Select(error => string.Format("{0}: {1}", error.Code, error.Message))));
            }
            catch (BulkOperationInProgressException ex)
            {
                OutputStatusMessage("The result file for the bulk operation is not yet available for download.");
                OutputStatusMessage(ex.Message);
            }
            catch (BulkOperationCouldNotBeCompletedException<DownloadStatus> ex)
            {
                OutputStatusMessage(string.Join("; ", ex.Errors.Select(error => string.Format("{0}: {1}", error.Code, error.Message))));
            }
            catch (BulkOperationCouldNotBeCompletedException<UploadStatus> ex)
            {
                OutputStatusMessage(string.Join("; ", ex.Errors.Select(error => string.Format("{0}: {1}", error.Code, error.Message))));
            }
            catch (Exception ex)
            {
                OutputStatusMessage(ex.Message);
            }
            finally
            {
                if (Reader != null) { Reader.Dispose(); }
                if (Writer != null) { Writer.Dispose(); }
            }
        }


    }
}
