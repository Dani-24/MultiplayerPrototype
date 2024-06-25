<?php

$servername = "localhost";
$username   = "danieltr1";
$password   = "aLZsQgYa6q9q";
$dbname     = "danieltr1";

if ($_SERVER["REQUEST_METHOD"] == "POST") {

    if(ConnectToServer() == false)  return;

    $methodToCall = $_POST["methodToCall"];

    switch((string)$methodToCall)
    {
        case "Log In":
            LogIn();
            break;
        case "Log Out":
            LogOut();
            break;
        case "Host Room":
            HostRoom();
            break;
        case "Close Room":
            CloseRoom();
            break;
        case "Search Room":
            SearchRoom();
            break;
        case "Send Host Data":
            SendHostData();
            break;
        case "Receive Host Data":
            ReceiveHostData();
            break;
        case "Send Client Data":
            SendClientData();
            break;
        case "Receive Client Data":
            ReceiveClientData();
            break;
        case "Disconnect Client":
            DisconnectClient();
            break;
        case "Send Extra Data":
            SendExtraData();
            break;
        case "Check Extra Data":
            CheckExtraData();
            break;
    }

    CloseConnection();
} 
else 
{
   echo "PHP: Method Error \n";
}

// LOG IN
function LogIn(){
    $userName      = $_POST["userName"];

    global $conn;
    $sql = "INSERT INTO Users (UserName) VALUES ('$userName')";

    if ($conn->query($sql) === TRUE)    echo $conn->insert_id;
    else                                echo "PHP: Logging Error " . mysqli_error($conn);
}

function LogOut(){
    $user_id = $_POST["userId"];

    global $conn;
    $sql = "DELETE FROM Users WHERE User_Id = $user_id";

    if ($conn->query($sql) === TRUE)    echo "PHP: User Unlogged";
    else                                echo "PHP: User Unlogging Error " . mysqli_error($conn);

    // Check
    $sql = "DELETE FROM ExtraData WHERE User_Id = $user_id";

    if ($conn->query($sql) === TRUE)    echo "PHP: Deleted extra data while Unlogging";
    else                                echo "PHP: No extra data to delete" . mysqli_error($conn);
}

// ROOMS
function HostRoom(){
    $timeStamp  = $_POST["timeStamp"];
    $host       = $_POST["host"];

    global $conn;

    $sql = "INSERT INTO Rooms (Host, Date) VALUES ('$host', '$timeStamp')";

    if ($conn->query($sql) === TRUE)    echo $conn->insert_id;
    else                                echo "PHP: Room Creation Error " . mysqli_error($conn);
}

function CloseRoom(){
    $id = $_POST["roomId"];

    global $conn;

    $sql = "DELETE FROM Rooms WHERE Room_Id = $id";

    if ($conn->query($sql) === TRUE)    echo "PHP: Room Deleted";
    else                                echo "PHP: Room Deletion Error " . mysqli_error($conn);

    $sql = "DELETE FROM HostData WHERE Room_Id = $id";

    if ($conn->query($sql) === TRUE)    echo "PHP: Room Data Deleted";
    else                                echo "PHP: Room Data Deletion Error " . mysqli_error($conn);

    LogOut();
}

function SearchRoom(){
    global $conn;

    $sql = "SELECT * FROM Rooms";

    $result = $conn->query($sql);

    if ($result->num_rows > 0) 
    {
        while ($row = $result->fetch_assoc()) 
            echo json_encode($row) . "\n";
    }
    else 
        echo "-1 No Rooms Available";
}

// HOST
function SendHostData(){
    $timeStamp  = $_POST["timeStamp"];
    $roomId     = $_POST["roomId"];
    $data       = $_POST["data"];

    global $conn;

    $sql = "SELECT * FROM HostData WHERE Room_Id = $roomId";

    $result = $conn->query($sql);

    if ($result->num_rows > 0)  $sql = "UPDATE HostData SET Data = '$data', Date = '$timeStamp' WHERE Room_Id = $roomId";
    else                        $sql = "INSERT INTO HostData (Room_Id, Data, Date) VALUES ('$roomId', '$data' , '$timeStamp')";                  

    if ($conn->query($sql) === TRUE)    echo "PHP: Host Data Updated";
    else                                echo "PHP: Error Sending Host Data " . mysqli_error($conn);
}

function ReceiveHostData(){
    $roomId = $_POST["roomId"];

    global $conn;

    $sql = "SELECT Data FROM HostData WHERE Room_Id = $roomId";

    $result = $conn->query($sql);

    if ($result->num_rows > 0)  echo $result->fetch_assoc()['Data'];
    else                        echo "PHP: No Host Data Available";
}

// CLIENT
function SendClientData(){
    $timeStamp  = $_POST["timeStamp"];
    $roomId     = $_POST["roomId"];
    $clientId   = $_POST["clientId"];
    $data       = $_POST["data"];

    global $conn;

    $sql = "SELECT * FROM ClientData WHERE Room_Id = $roomId AND Client_Id = $clientId";

    $result = $conn->query($sql);

    if ($result->num_rows > 0)  $sql = "UPDATE ClientData SET Data = '$data', Date = '$timeStamp' WHERE Room_Id = $roomId AND Client_Id = $clientId";
    else                        $sql = "INSERT INTO ClientData (Room_Id, Client_Id, Data, Date) VALUES ('$roomId', '$clientId', '$data', '$timeStamp')";

    if ($conn->query($sql) === TRUE)    echo "PHP: Client Data Sent";
    else                                echo "PHP: Error Sending Client Data " . mysqli_error($conn);
}

function ReceiveClientData(){
    $roomId = $_POST["roomId"];

    global $conn;

    $sql = "SELECT * FROM ClientData WHERE Room_Id = $roomId";

    $result = $conn->query($sql);

    if ($result->num_rows > 0) 
    {
        while ($row = $result->fetch_assoc()) 
            echo json_encode($row) . "\n";
    }
    else 
        echo "No Client Data Available";
}

function DisconnectClient(){
    $id = $_POST["roomId"];

    global $conn;

    $sql = "DELETE FROM ClientData WHERE Room_Id = $id";

    if ($conn->query($sql) === TRUE)    echo "PHP: Client Data Deleted";
    else                                echo "PHP: Client Data Deletion Error " . mysqli_error($conn);

    LogOut();
}

// Additional Packages
function SendExtraData(){
    $timeStamp  = $_POST["timeStamp"];
    $roomId     = $_POST["roomId"];
    $userId     = $_POST["User_Id"];
    $data       = $_POST["data"];

    global $conn;

    $sql = "INSERT INTO ExtraData (User_Id, Room_Id, Data, Date) VALUES ('$userId', '$roomId', '$data', '$timeStamp')";

    if ($conn->query($sql) === TRUE)    echo "PHP: Extra Data Uploaded";
    else                                echo "PHP: Error Sending Extra Data " . mysqli_error($conn);
}

function CheckExtraData(){
    $roomId = $_POST["Room_Id"];
    $askId = $_POST["Ask_Id"];
    $userId = $_POST["User_Id"];

    global $conn;

    $sql = "SELECT * FROM ExtraData WHERE Id > $askId AND User_Id != $userId AND Room_Id = $roomId ORDER BY Date LIMIT 20";

    $result = $conn->query($sql);

    if ($result->num_rows > 0) 
    {
        while ($row = $result->fetch_assoc())
            echo json_encode($row) . "\n";
    }
    else    echo "PHP: No Extra Data Available";
}

// Connection
function ConnectToServer() 
{
   global $servername, $username, $password, $dbname;
   global $conn;
   
   $conn = new mysqli($servername, $username, $password, $dbname);

   if ($conn->connect_error) {
       die("PHP: Connection Error: " . $conn->connect_error);
   }

   return true;
}

function CloseConnection() 
{
   global $conn;
   $conn->close();
}

?>