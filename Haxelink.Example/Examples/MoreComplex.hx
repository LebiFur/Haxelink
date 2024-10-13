class Main {
    public static function main() {
        while (true) {
            var point = new Point(Math.floor(Math.random() * 100), Math.floor(Math.random() * 100));
            trace(point.toString());
        }
    }
}

enum Color {
    Red;
    Green;
    Blue;
    Rgb(r:Float, g:Float, b:Float);
}

class Point {
    var x:Int;
    var y:Int;
    var color:Color;
  
    public function new(x, y) {
        this.x = x;
        this.y = y;
        color = if (Math.random() > 0.5) {
            switch(Math.round(Math.random() * 2)) {
                case 0: Red;
                case 1: Green;
                default: Blue;
            }
        } else {
            Rgb(Math.random(), Math.random(), Math.random());
        }
    }
  
    public function toString() {
        return "Point(" + x + ", " + y + " | " + color + ")";
    }
}