<?php

header("Content-Type: text/html; charset=utf-8");
print '<html><title>Flush Test</title><head></head><body> <p>Starting...</p>';
function flush_buffers(){
  ob_flush();
  flush();
}
flush_buffers();
for($i = 0; $i < 10; $i++) {
  print   "$i<br/>\n";
  flush_buffers();
  sleep(1);
}
flush_buffers();
print "DONE!<br/>\n";
print '</body></html>';
