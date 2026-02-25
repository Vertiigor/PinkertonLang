# Pinkerton

> A small interpreted language with expressive syntax, functional pipelines, ranges, and first-class functions.

Pinkerton is an experimental programming language featuring:

* Clean and readable syntax
* Functional-style pipelines (`|>`)
* Range expressions (`0..10`, `'a'..'z'`)
* First-class functions
* One-line function declarations
* Arrays and indexing
* Control flow statements
* Native and user-defined functions

---

# ✨ Example

```pinkerton
function isEven(x) := x mod 2 = 0
function powTwo(x) := pow(x, 2)

let result :=
    0..20
    |> filter(isEven)
    |> map(powTwo)
    |> sorted()

writeln join(result, ';')
```

---

# 📦 Features

---

## 🔢 Variables

```pinkerton
let x := 10
let name := "Pinkerton"
let flag := true
```

---

## 🧮 Expressions

```pinkerton
let a := 5 + 2 * 3
let b := (5 + 2) * 3
let c := 10 mod 3
let d := 2 ^ 8
let e := true and not false
```

---

## 📏 Ranges

```pinkerton
let numbers := 0..10
let stepRange := 0..20 step 2

let lowercase := 'a'..'z'
let uppercase := 'A'..'Z'
```

Ranges can also be combined:

```pinkerton
let letters := ('a'..'z') ^ ('A'..'Z')
```

---

## 🔗 Pipelines

Pipeline operator `|>` passes the result of the left expression
as the first argument of the function on the right.

```pinkerton
let result :=
    0..50
    |> filter(isEven)
    |> map(powTwo)
    |> sorted()
```

Equivalent to:

```pinkerton
sorted(map(filter(0..50, isEven), powTwo))
```

---

## 🧠 Functions

### One-line function

```pinkerton
function square(x) := x * x
```

### Multi-line function

```pinkerton
function factorial(n)
begin
    if n = 0 then
        return 1

    return n * factorial(n - 1)
end
```

Functions are first-class values and can be passed as arguments.

---

## 📚 Arrays

```pinkerton
let arr := [1; 2; 3; 4]

writeln arr[0]
```

---

## 🔎 Membership Operator

```pinkerton
let letters := 'a'..'z'

if 'c' in letters then
    writeln "Found!"
```

---

## 🔀 Conditional Expression

```pinkerton
let result := select if x > 0 then "positive" else "negative"
```

---

## 🔁 Control Flow

### If

```pinkerton
if x > 10 then
    writeln "Big"
else
    writeln "Small"
```

### While

```pinkerton
while x > 0 do
begin
    writeln x
    x := x - 1
end
```
---

## 🧵 Functional Style

Pinkerton encourages functional composition:

```pinkerton
let alphabet :=
    ('a'..'z')
    |> filter(isLetter)
    |> sorted()
```

---

# 🏗 Architecture

Pinkerton is implemented as:

* Lexer
* Recursive descent parser
* AST interpreter
* Support for native and user-defined functions

Pipeline expressions are transformed at parse time into regular function calls.

---

# 🚀 Running

```bash
Pinkerton> run File.pink
```

---

# 🎯 Design Goals

* Minimal syntax
* Expressive range operations
* Functional composition
* Simple interpreter architecture
* Educational compiler/interpreter project

---

# 📌 Example Program

```pinkerton
function isPrime(n)
{
    if n < 2 then return false

    let i := 2
    while i * i <= n do
    {
        if n mod i = 0 then
            return false
        i := i + 1
    }

    return true
}

let primes :=
    2..100
    |> filter(isPrime)

println primes
```

---

# 📜 License

MIT
