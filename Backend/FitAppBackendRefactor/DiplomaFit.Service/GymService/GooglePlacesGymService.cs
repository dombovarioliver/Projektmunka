using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Gym;
using DiplomaFit.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DiplomaFit.Service.GymService
{
    public class GooglePlacesGymService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly string _apiKey;

        public GooglePlacesGymService(
            HttpClient httpClient,
            AppDbContext dbContext,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;

            _apiKey = configuration["GooglePlaces:ApiKey"]
                ?? throw new InvalidOperationException("GooglePlaces:ApiKey nincs beállítva.");
        }

        public async Task<List<GymDto>> GetBudapestGymsAsync(
            CancellationToken cancellationToken = default)
        {
            var existingGyms = await _dbContext.Gyms
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .ToListAsync(cancellationToken);

            if (existingGyms.Any())
            {
                return existingGyms
                    .Select(MapToDto)
                    .ToList();
            }

            var gymsFromGoogle = await FetchBudapestGymsFromGoogleAsync(cancellationToken);

            var gymEntities = gymsFromGoogle
                .Where(g => !string.IsNullOrWhiteSpace(g.GymId))
                .Select(dto => new Gym
                {
                    GymId = dto.GymId,
                    Name = dto.Name,
                    Address = dto.Address,
                    Rating = dto.Rating,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                })
                .ToList();

            if (gymEntities.Any())
            {
                await _dbContext.Gyms.AddRangeAsync(gymEntities, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return gymEntities
                .OrderBy(g => g.Name)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<List<GymDto>> RefreshBudapestGymsAsync(
            CancellationToken cancellationToken = default)
        {
            var gymsFromGoogle = await FetchBudapestGymsFromGoogleAsync(cancellationToken);

            foreach (var dto in gymsFromGoogle)
            {
                if (string.IsNullOrWhiteSpace(dto.GymId))
                    continue;

                var existingGym = await _dbContext.Gyms
                    .FirstOrDefaultAsync(g => g.GymId == dto.GymId, cancellationToken);

                if (existingGym == null)
                {
                    _dbContext.Gyms.Add(new Gym
                    {
                        GymId = dto.GymId,
                        Name = dto.Name,
                        Address = dto.Address,
                        Rating = dto.Rating,
                        Latitude = dto.Latitude,
                        Longitude = dto.Longitude,
                    });
                }
                else
                {
                    existingGym.Name = dto.Name;
                    existingGym.Address = dto.Address;
                    existingGym.Rating = dto.Rating;
                    existingGym.Latitude = dto.Latitude;
                    existingGym.Longitude = dto.Longitude;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await _dbContext.Gyms
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => MapToDto(g))
                .ToListAsync(cancellationToken);
        }

        private async Task<List<GymDto>> FetchBudapestGymsFromGoogleAsync(
            CancellationToken cancellationToken)
        {
            var requestBody = new
            {
                includedTypes = new[] { "gym" },
                maxResultCount = 20,
                locationRestriction = new
                {
                    circle = new
                    {
                        center = new
                        {
                            latitude = 47.4979,
                            longitude = 19.0402
                        },
                        radius = 25000.0
                    }
                },
                languageCode = "hu",
                regionCode = "HU"
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://places.googleapis.com/v1/places:searchNearby"
            );

            request.Headers.Add("X-Goog-Api-Key", _apiKey);
            request.Headers.Add(
                "X-Goog-FieldMask",
                "places.id,places.displayName,places.formattedAddress,places.rating,places.location"
            );

            request.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Google Places API hiba: {response.StatusCode} - {error}");
            }

            var googleResponse = await response.Content
                .ReadFromJsonAsync<GooglePlacesResponse>(
                    cancellationToken: cancellationToken
                );

            return googleResponse?.Places?
                .Where(p => p.Location != null)
                .Select(p => new GymDto
                {
                    GymId = p.Id ?? string.Empty,
                    Name = p.DisplayName?.Text ?? "Ismeretlen konditerem",
                    Address = p.FormattedAddress ?? string.Empty,
                    Rating = p.Rating,
                    Latitude = p.Location!.Latitude,
                    Longitude = p.Location.Longitude
                })
                .ToList() ?? new List<GymDto>();
        }

        private static GymDto MapToDto(Gym gym)
        {
            return new GymDto
            {
                GymId = gym.GymId,
                Name = gym.Name,
                Address = gym.Address,
                Rating = gym.Rating,
                Latitude = gym.Latitude,
                Longitude = gym.Longitude
            };
        }

        private class GooglePlacesResponse
        {
            [JsonPropertyName("places")]
            public List<GooglePlace>? Places { get; set; }
        }

        private class GooglePlace
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("displayName")]
            public GoogleDisplayName? DisplayName { get; set; }

            [JsonPropertyName("formattedAddress")]
            public string? FormattedAddress { get; set; }

            [JsonPropertyName("rating")]
            public double? Rating { get; set; }

            [JsonPropertyName("location")]
            public GoogleLocation? Location { get; set; }
        }

        private class GoogleDisplayName
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }

        private class GoogleLocation
        {
            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }
        }
    }
}