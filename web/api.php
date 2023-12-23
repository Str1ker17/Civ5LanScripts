<?php

$ret["status"] = 0;
$ret["msg"] = "OK";

function do_latency_test($cmd, &$ret) {
    $ret["src_ip"] = $_SERVER["REMOTE_ADDR"];
    exec($cmd, $output, $rc);
    if ($rc != 0) {
        $ret["latency_min"] = -1;
        $ret["latency_avg"] = -1;
        $ret["latency_max"] = -1;
    } else {
        if (preg_match("~^round-trip min/avg/max = ([\d]+)\.[\d]+/([\d]+)\.[\d]+/([\d]+)\.[\d]+ ms$~s", $output[4], $m) != 1) {
            throw new Exception("Server ping command output format mismatch");
        }
        $ret["latency_min"] = intval($m[1]);
        $ret["latency_avg"] = intval($m[2]);
        $ret["latency_max"] = intval($m[3]);
    }
}

try {
    $post_body = file_get_contents("php://input");
    if (($req = json_decode($post_body, true)) === false)
        throw new Exception("Invalid JSON request");
    switch($req["func"]) {
        case "ping": {
            $cmd = sprintf("ping -q -s 4 -A -W 1 -c 1 -I tap-udp -4 %s", $_SERVER["REMOTE_ADDR"]);
            do_latency_test($cmd, $ret);
            break;
        }
        case "latency": {
            $cmd = sprintf("ping -q -s 4 -A -W 1 -w 2 -I tap-udp -4 %s", $_SERVER["REMOTE_ADDR"]);
            do_latency_test($cmd, $ret);
            break;
        }
        default: {
            throw new Exception("Unsupported func '${req["func"]}'");
        }
    }
}
catch(Exception $e) {
    $ret["status"] = 1;
    $ret["msg"] = $e->getMessage();
}

http_response_code($ret["status"] == 0 ? 200 : 403);
header("Content-Type: application/json");
print json_encode($ret);
exit(0);
