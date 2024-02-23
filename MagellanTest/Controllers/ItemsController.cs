using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Data;

namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly string _connectionString;

        public ItemsController()
        {
            _connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=142022;Database=Part;";
            
        }
        // Added JSON item is sent in request body when making the POST request
        // POST /items
        [HttpPost]
        public IActionResult CreateItem([FromBody] ItemData itemData)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                using var cmd = new NpgsqlCommand("INSERT INTO item (item_name, parent_item, cost, req_date) VALUES (@itemName, @parentItem, @cost, @reqDate) RETURNING id", conn);
                cmd.Parameters.AddWithValue("itemName", itemData.ItemName);
                if (itemData.ParentItem != null) {
                    cmd.Parameters.AddWithValue("parentItem", itemData.ParentItem);
                } else {
                    cmd.Parameters.AddWithValue("parentItem", DBNull.Value);
                }
                cmd.Parameters.AddWithValue("cost", itemData.Cost);
                cmd.Parameters.AddWithValue("reqDate", itemData.ReqDate);
                var itemId = (int)cmd.ExecuteScalar();
                return Ok(itemId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //Get the item by id(int)
        //GET /items/{id}
        [HttpGet("{id}")]
        public IActionResult GetItem(int id)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT * FROM item WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var item = new
                    {
                        id = reader.GetInt32(0),
                        item_name = reader.GetString(1),
                        parent_item = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                        cost = reader.GetInt32(3),
                        req_date = reader.GetDateTime(4)
                    };
                    return Ok(item);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //Get the total cost of an item, return null if it is a sub
        //GET /items/totalcost/{itemName}
        [HttpGet("totalcost/{itemName}")]
        public IActionResult GetTotalCost(string itemName)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT Get_Total_Cost(@itemName)", conn);
                cmd.Parameters.AddWithValue("itemName", itemName);
                var totalCost = cmd.ExecuteScalar();
                if (totalCost == DBNull.Value) {
                    return Ok(null);
                }
                return Ok((int)totalCost);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class ItemData
    {
        public string ItemName { get; set; }
        public int? ParentItem { get; set; }
        public int Cost { get; set; }
        public DateTime ReqDate { get; set; }
    }
}
