-- Create the Part database
CREATE DATABASE Part;

-- Connect to the Part database
\c Part

-- Create the item table
CREATE TABLE item (
    id SERIAL PRIMARY KEY,
    item_name VARCHAR(50) NOT NULL,
    parent_item INTEGER,
    cost INTEGER NOT NULL,
    req_date DATE NOT NULL,
    FOREIGN KEY (parent_item) REFERENCES item(id)
);

-- Insert data into the item table
INSERT INTO item (item_name, parent_item, cost, req_date) VALUES
('Item1', null, 500, '2024-02-20'),
('Sub1', 1, 200, '2024-02-10'),
('Sub2', 1, 300, '2024-01-05'),
('Sub3', 2, 300, '2024-01-02'),
('Sub4', 2, 400, '2024-01-02'),
('Item2', null, 600, '2024-03-15'),
('Sub1', 6, 200, '2024-02-25');

-- Create function Get_Total_Cost
CREATE OR REPLACE FUNCTION Get_Total_Cost(item_name_param VARCHAR(50))
RETURNS INTEGER AS $$
DECLARE
    total_cost INTEGER := 0;
    parent_item_exists BOOLEAN;
BEGIN
    -- Check if the provided item has a parent item
    SELECT EXISTS (SELECT 0 FROM item WHERE item_name = item_name_param AND parent_item IS NOT NULL) INTO parent_item_exists;
    
    -- If the item has a parent item, return NULL
    IF parent_item_exists THEN
        RETURN NULL;
    END IF;

    -- Otherwise, calculate the total cost recursively
    WITH RECURSIVE ItemHierarchy AS (
        SELECT id, cost
        FROM item
        WHERE item_name = item_name_param
        UNION ALL
        SELECT i.id, i.cost
        FROM item i
        JOIN ItemHierarchy ih ON ih.id = i.parent_item
    )
    SELECT SUM(cost) INTO total_cost
    FROM ItemHierarchy;

    RETURN total_cost;
END;
$$ LANGUAGE plpgsql;

-- -- Test the function
-- SELECT Get_Total_Cost('Sub1') AS total_cost_sub1;
-- SELECT Get_Total_Cost('Item1') AS total_cost_item1;
